using System;
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
        private readonly Dictionary<Type, object> _serverEvents = [];
        private readonly Dictionary<Type, Dictionary<object, AsyncEventPriority>> _postHandlers = [];
        private readonly Dictionary<Type, Dictionary<object, AsyncEventPriority>> _preHandlers = [];

        /// <inheritdoc cref="AsyncEvent{T}.ParallelizationEnabled"/>
        public bool ParallelizationEnabled { get; init; }

        /// <inheritdoc cref="AsyncEvent{T}.MinimumParallelHandlerCount"/>
        public int MinimumParallelHandlerCount { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncEventContainer"/> class, which contains asynchronous events and event handlers.
        /// </summary>
        public AsyncEventContainer() : this(false, 0) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncEventContainer"/> class, which contains asynchronous events and event handlers.
        /// </summary>
        /// <inheritdoc cref="AsyncEvent{T}.AsyncEvent(bool)"/>
        public AsyncEventContainer(bool parallelize) : this(parallelize, Environment.ProcessorCount) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncEventContainer"/> class, which contains asynchronous events and event handlers.
        /// </summary>
        /// <inheritdoc cref="AsyncEvent{T}.AsyncEvent(bool, int)"/>
        public AsyncEventContainer(bool parallelize, int minimumParallelHandlers)
        {
            ParallelizationEnabled = parallelize;
            MinimumParallelHandlerCount = minimumParallelHandlers;
        }

        /// <summary>
        /// Finds or lazily creates an asynchronous event of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the asynchronous event arguments.</typeparam>
        /// <returns>A prepared asynchronous event of the specified type with the appropriate handlers.</returns>
        public AsyncEvent<T> GetAsyncEvent<T>() where T : AsyncEventArgs
        {
            if (_serverEvents.TryGetValue(typeof(T), out object? value))
            {
                return (AsyncEvent<T>)value;
            }

            AsyncEvent<T> asyncServerEvent = new(ParallelizationEnabled, MinimumParallelHandlerCount);
            if (_preHandlers.TryGetValue(typeof(T), out Dictionary<object, AsyncEventPriority>? preHandlers))
            {
                foreach ((object preHandler, AsyncEventPriority priority) in preHandlers)
                {
                    // Cannot use 'preHandler' as a ref or out value because it is a 'foreach iteration variable' csharp(CS1657)
                    asyncServerEvent.AddPreHandler((AsyncEventPreHandler<T>)preHandler, priority);
                }
            }

            if (_postHandlers.TryGetValue(typeof(T), out Dictionary<object, AsyncEventPriority>? postHandlers))
            {
                foreach ((object postHandler, AsyncEventPriority priority) in postHandlers)
                {
                    // Cannot use 'postHandler' as a ref or out value because it is a 'foreach iteration variable' csharp(CS1657)
                    asyncServerEvent.AddPostHandler((AsyncEventPostHandler<T>)postHandler, priority);
                }
            }

            asyncServerEvent.Prepare();
            _serverEvents.Add(typeof(T), asyncServerEvent);
            return asyncServerEvent;
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

        /// <inheritdoc cref="AsyncEvent{TEventArgs}.AddHandlers{T}(object?)"/>
        public void AddHandlers<T>(object? target = null) => AddHandlers(typeof(T), target);

        /// <inheritdoc cref="AsyncEvent{TEventArgs}.AddHandlers(Type, object?)"/>
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
                if (parameters.Length != 1 || !parameters[0].ParameterType.IsSubclassOf(typeof(AsyncEventArgs)))
                {
                    continue;
                }

                if (method.ReturnType == typeof(ValueTask<bool>))
                {
                    Type genericMethodType = typeof(AsyncEventPreHandler<>).MakeGenericType(parameters[0].ParameterType);
                    if (method.IsStatic)
                    {
                        AddPreHandler((AsyncEventPreHandler)Delegate.CreateDelegate(genericMethodType, method), attribute.Priority, parameters[0].ParameterType);
                    }
                    else if (target is not null)
                    {
                        AddPreHandler((AsyncEventPreHandler)Delegate.CreateDelegate(genericMethodType, target, method), attribute.Priority, parameters[0].ParameterType);
                    }
                }
                else
                {
                    Type genericMethodType = typeof(AsyncEventPostHandler<>).MakeGenericType(parameters[0].ParameterType);
                    if (method.IsStatic)
                    {
                        AddPostHandler((AsyncEventPostHandler)Delegate.CreateDelegate(genericMethodType, method), attribute.Priority, parameters[0].ParameterType);
                    }
                    else if (target is not null)
                    {
                        AddPostHandler((AsyncEventPostHandler)Delegate.CreateDelegate(genericMethodType, target, method), attribute.Priority, parameters[0].ParameterType);
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
            if (!_preHandlers.TryGetValue(typeof(T), out Dictionary<object, AsyncEventPriority>? preHandlers))
            {
                preHandlers = [];
                _preHandlers.Add(typeof(T), preHandlers);
            }

            preHandlers.Add(preHandler, priority);
        }

        /// <summary>
        /// Registers an asynchronous event handler for the specified event type.
        /// </summary>
        /// <param name="preHandler">The asynchronous event handler to register.</param>
        /// <param name="priority">The priority of the event handler.</param>
        /// <param name="type">The type of the asynchronous event arguments.</param>
        public void AddPreHandler(AsyncEventPreHandler preHandler, AsyncEventPriority priority, Type type)
        {
            // Check if the type implements AsyncEventArgs
            if (type != typeof(AsyncEventArgs) && type.IsAssignableFrom(typeof(AsyncEventArgs)))
            {
                throw new ArgumentException($"Type must implement {nameof(AsyncEventArgs)}", nameof(type));
            }

            if (!_preHandlers.TryGetValue(type, out Dictionary<object, AsyncEventPriority>? preHandlers))
            {
                preHandlers = [];
                _preHandlers.Add(type, preHandlers);
            }

            preHandlers.Add(preHandler, priority);
        }

        /// <summary>
        /// Registers an asynchronous event handler for the specified event type.
        /// </summary>
        /// <param name="postHandler">The asynchronous event handler to register.</param>
        /// <param name="priority">The priority of the event handler.</param>
        /// <typeparam name="T">The type of the asynchronous event arguments.</typeparam>
        public void AddPostHandler<T>(AsyncEventPostHandler<T> postHandler, AsyncEventPriority priority) where T : AsyncEventArgs
        {
            if (!_postHandlers.TryGetValue(typeof(T), out Dictionary<object, AsyncEventPriority>? postHandlers))
            {
                postHandlers = [];
                _postHandlers.Add(typeof(T), postHandlers);
            }

            postHandlers.Add(postHandler, priority);
        }

        /// <summary>
        /// Registers an asynchronous event handler for the specified event type.
        /// </summary>
        /// <param name="postHandler">The asynchronous event handler to register.</param>
        /// <param name="priority">The priority of the event handler.</param>
        /// <param name="type">The type of the asynchronous event arguments.</param>
        public void AddPostHandler(AsyncEventPostHandler postHandler, AsyncEventPriority priority, Type type)
        {
            if (type != typeof(AsyncEventArgs) && type.IsAssignableFrom(typeof(AsyncEventArgs)))
            {
                throw new ArgumentException($"Type must implement {nameof(AsyncEventArgs)}", nameof(type));
            }

            if (!_postHandlers.TryGetValue(type, out Dictionary<object, AsyncEventPriority>? postHandlers))
            {
                postHandlers = [];
                _postHandlers.Add(type, postHandlers);
            }

            postHandlers.Add(postHandler, priority);
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

            _preHandlers.Remove(type);
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

            _postHandlers.Remove(type);
        }

        /// <summary>
        /// Removes all post-event handlers for all event types.
        /// </summary>
        public void ClearPostHandlers() => _postHandlers.Clear();
    }
}
