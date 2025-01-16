using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    /// <summary>
    /// An object that contains asynchronous events and event handlers.
    /// Intended to be used for dependency injection as a singleton or in a similar manner.
    /// </summary>
    public sealed class AsyncEventContainer
    {
        private static readonly MethodInfo _getAsyncEventGenericMethod = typeof(AsyncEventContainer).GetMethod(nameof(GetAsyncEvent), BindingFlags.Public | BindingFlags.Instance, [])!;
        private static readonly MethodInfo _addPreHandlerGenericMethod = typeof(AsyncEventContainer).GetMethod(nameof(AddPreHandler), BindingFlags.Public | BindingFlags.Instance)!;
        private static readonly MethodInfo _addPostHandlerGenericMethod = typeof(AsyncEventContainer).GetMethod(nameof(AddPostHandler), BindingFlags.Public | BindingFlags.Instance)!;

        private readonly ConcurrentDictionary<Type, IAsyncEvent> _serverEvents = [];
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<object, AsyncEventPriority>> _postHandlers = [];
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<object, AsyncEventPriority>> _preHandlers = [];

        /// <summary>
        /// Finds or lazily creates an asynchronous event of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the asynchronous event arguments.</typeparam>
        /// <returns>A prepared asynchronous event of the specified type with the appropriate handlers.</returns>
        public AsyncEvent<T> GetAsyncEvent<T>() where T : AsyncEventArgs
        {
            if (_serverEvents.TryGetValue(typeof(T), out IAsyncEvent? value))
            {
                return (AsyncEvent<T>)value;
            }

            AsyncEvent<T> asyncServerEvent = new();
            if (_preHandlers.TryGetValue(typeof(T), out ConcurrentDictionary<object, AsyncEventPriority>? preHandlers))
            {
                foreach ((object preHandler, AsyncEventPriority priority) in preHandlers)
                {
                    // Cannot use 'preHandler' as a ref or out value because it is a 'foreach iteration variable' csharp(CS1657)
                    asyncServerEvent.AddPreHandler((AsyncEventPreHandler<T>)preHandler, priority);
                }
            }

            if (_postHandlers.TryGetValue(typeof(T), out ConcurrentDictionary<object, AsyncEventPriority>? postHandlers))
            {
                foreach ((object postHandler, AsyncEventPriority priority) in postHandlers)
                {
                    // Cannot use 'postHandler' as a ref or out value because it is a 'foreach iteration variable' csharp(CS1657)
                    asyncServerEvent.AddPostHandler((AsyncEventPostHandler<T>)postHandler, priority);
                }
            }

            asyncServerEvent.Prepare();
            _serverEvents.TryAdd(typeof(T), asyncServerEvent);
            return asyncServerEvent;
        }

        /// <summary>
        /// Finds or lazily creates an asynchronous event of the specified type.
        /// </summary>
        /// <remarks>
        /// If the async event does not exist, there will be a notable performance
        /// hit as the event is created and prepared through reflection. Try to use
        /// this method only when you know the event exists. Otherwise, prefer using
        /// the generic method <see cref="GetAsyncEvent{T}"/>.
        /// </remarks>
        /// <param name="type">The type of the asynchronous event arguments.</param>
        /// <returns>A prepared asynchronous event of the specified type with the appropriate handlers.</returns>
        public IAsyncEvent GetAsyncEvent(Type type)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));
            if (type != typeof(AsyncEventArgs) && !type.IsSubclassOf(typeof(AsyncEventArgs)))
            {
                throw new ArgumentException($"Type must implement {nameof(AsyncEventArgs)}", nameof(type));
            }
            else if (_serverEvents.TryGetValue(type, out IAsyncEvent? value))
            {
                return value;
            }

            // Call GetAsyncEvent<T> through reflection
            MethodInfo genericMethod = _getAsyncEventGenericMethod.MakeGenericMethod(type);
            return (IAsyncEvent)genericMethod.Invoke(this, [])!;
        }

        /// <summary>
        /// Compiles the pre/post event handlers into a single delegate for faster execution. This should be called after any handlers have been modified.
        /// </summary>
        /// <typeparam name="T">The type of the asynchronous event arguments.</typeparam>
        public void Prepare<T>() where T : AsyncEventArgs
        {
            AsyncEvent<T> asyncServerEvent = GetAsyncEvent<T>();
            asyncServerEvent.ClearPreHandlers();
            asyncServerEvent.ClearPostHandlers();

            foreach ((object preHandler, AsyncEventPriority priority) in _preHandlers[typeof(T)])
            {
                asyncServerEvent.AddPreHandler((AsyncEventPreHandler<T>)preHandler, priority);
            }

            foreach ((object postHandler, AsyncEventPriority priority) in _postHandlers[typeof(T)])
            {
                asyncServerEvent.AddPostHandler((AsyncEventPostHandler<T>)postHandler, priority);
            }

            asyncServerEvent.Prepare();
        }

        /// <inheritdoc cref="IAsyncEvent{TEventArgs}.AddHandlers{T}(object?)"/>
        public void AddHandlers<T>(object? target = null) => AddHandlers(typeof(T), target);

        /// <inheritdoc cref="IAsyncEvent{TEventArgs}.AddHandlers(Type, object?)"/>
        public void AddHandlers(Type type, object? target = null)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // If the method does not have the AsyncEventHandlerAttribute, skip the method
                AsyncEventHandlerAttribute? attribute = method.GetCustomAttribute<AsyncEventHandlerAttribute>();
                if (attribute is null)
                {
                    continue;
                }

                // If the return type is not a ValueTask or ValueTask<bool>, skip the method
                if (method.ReturnType != typeof(ValueTask) && method.ReturnType != typeof(ValueTask<bool>))
                {
                    continue;
                }

                // Else if the method's signature does not match the expected signature, skip the method
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != 1 || (parameters[0].ParameterType != typeof(AsyncEventArgs) && !parameters[0].ParameterType.IsSubclassOf(typeof(AsyncEventArgs))))
                {
                    continue;
                }

                if (method.ReturnType == typeof(ValueTask<bool>))
                {
                    Type genericMethodType = typeof(AsyncEventPreHandler<>).MakeGenericType(parameters[0].ParameterType);
                    if (method.IsStatic)
                    {
                        _addPreHandlerGenericMethod.MakeGenericMethod(parameters[0].ParameterType).Invoke(this, [Delegate.CreateDelegate(genericMethodType, method), attribute.Priority]);
                    }
                    else if (target is not null)
                    {
                        _addPreHandlerGenericMethod.MakeGenericMethod(parameters[0].ParameterType).Invoke(this, [Delegate.CreateDelegate(genericMethodType, target, method), attribute.Priority]);
                    }
                }
                else
                {
                    Type genericMethodType = typeof(AsyncEventPostHandler<>).MakeGenericType(parameters[0].ParameterType);
                    if (method.IsStatic)
                    {
                        _addPostHandlerGenericMethod.MakeGenericMethod(parameters[0].ParameterType).Invoke(this, [Delegate.CreateDelegate(genericMethodType, method), attribute.Priority]);
                    }
                    else if (target is not null)
                    {
                        _addPostHandlerGenericMethod.MakeGenericMethod(parameters[0].ParameterType).Invoke(this, [Delegate.CreateDelegate(genericMethodType, target, method), attribute.Priority]);
                    }
                }
            }
        }

        /// <summary>
        /// Registers an asynchronous event handler for the specified event type.
        /// </summary>
        /// <param name="preHandler">The asynchronous event handler to register.</param>
        /// <param name="priority">The priority of the event handler.</param>
        /// <typeparam name="T">The type of the asynchronous event arguments.</typeparam>
        public void AddPreHandler<T>(AsyncEventPreHandler<T> preHandler, AsyncEventPriority priority) where T : AsyncEventArgs
        {
            ConcurrentDictionary<object, AsyncEventPriority> preHandlers = _preHandlers.GetOrAdd(typeof(T), []);
            preHandlers.AddOrUpdate(preHandler, priority, (_, _) => priority);
        }

        /// <summary>
        /// Registers an asynchronous event handler for the specified event type.
        /// </summary>
        /// <param name="postHandler">The asynchronous event handler to register.</param>
        /// <param name="priority">The priority of the event handler.</param>
        /// <typeparam name="T">The type of the asynchronous event arguments.</typeparam>
        public void AddPostHandler<T>(AsyncEventPostHandler<T> postHandler, AsyncEventPriority priority) where T : AsyncEventArgs
        {
            ConcurrentDictionary<object, AsyncEventPriority> postHandlers = _postHandlers.GetOrAdd(typeof(T), []);
            postHandlers.AddOrUpdate(postHandler, priority, (_, _) => priority);
        }

        /// <summary>
        /// Removes all pre-event handlers for the specified event type.
        /// </summary>
        /// <typeparam name="T">The type of the asynchronous event arguments.</typeparam>
        public void ClearPreHandlers<T>() where T : AsyncEventArgs => ClearPreHandlers(typeof(T));

        /// <summary>
        /// Removes all pre-event handlers for the specified event type.
        /// </summary>
        /// <param name="type">The type of the asynchronous event arguments.</param>
        public void ClearPreHandlers(Type type)
        {
            if (type != typeof(AsyncEventArgs) && type.IsAssignableFrom(typeof(AsyncEventArgs)))
            {
                throw new ArgumentException($"Type must implement {nameof(AsyncEventArgs)}", nameof(type));
            }

            _preHandlers.Remove(type, out _);
        }

        /// <summary>
        /// Removes all pre-event handlers for all event types.
        /// </summary>
        public void ClearPreHandlers() => _preHandlers.Clear();

        /// <summary>
        /// Removes all post-event handlers for the specified event type.
        /// </summary>
        /// <typeparam name="T">The type of the asynchronous event arguments.</typeparam>
        public void ClearPostHandlers<T>() where T : AsyncEventArgs => ClearPostHandlers(typeof(T));

        /// <summary>
        /// Removes all post-event handlers for the specified event type.
        /// </summary>
        /// <param name="type">The type of the asynchronous event arguments.</param>
        public void ClearPostHandlers(Type type)
        {
            if (type != typeof(AsyncEventArgs) && type.IsAssignableFrom(typeof(AsyncEventArgs)))
            {
                throw new ArgumentException($"Type must implement {nameof(AsyncEventArgs)}", nameof(type));
            }

            _postHandlers.Remove(type, out _);
        }

        /// <summary>
        /// Removes all post-event handlers for all event types.
        /// </summary>
        public void ClearPostHandlers() => _postHandlers.Clear();
    }
}
