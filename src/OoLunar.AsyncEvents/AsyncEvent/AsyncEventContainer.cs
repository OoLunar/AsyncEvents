using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace OoLunar.AsyncEvents
{
    /// <summary>
    /// An object that contains asynchronous events and event handlers.
    /// Intended to be used for dependency injection as a singleton or in a similar manner.
    /// </summary>
    public class AsyncEventContainer : IAsyncEventContainer
    {
        private static readonly MethodInfo _getAsyncEventGenericMethod = typeof(AsyncEventContainer).GetMethod(nameof(GetAsyncEvent), BindingFlags.Public | BindingFlags.Instance, [])!;
        private static readonly MethodInfo _prepareGenericMethod = typeof(AsyncEventContainer).GetMethod(nameof(Prepare), BindingFlags.Public | BindingFlags.Instance, [])!;
        private static readonly MethodInfo _addPreHandlerGenericMethod = typeof(AsyncEventContainer).GetMethod(nameof(AddPreHandler), BindingFlags.Public | BindingFlags.Instance)!;
        private static readonly MethodInfo _addPostHandlerGenericMethod = typeof(AsyncEventContainer).GetMethod(nameof(AddPostHandler), BindingFlags.Public | BindingFlags.Instance)!;
        private static readonly MethodInfo _clearPreHandlersGenericMethod = typeof(AsyncEventContainer).GetMethod(nameof(ClearPreHandlers), BindingFlags.Public | BindingFlags.Instance, [])!;
        private static readonly MethodInfo _clearPostHandlersGenericMethod = typeof(AsyncEventContainer).GetMethod(nameof(ClearPostHandlers), BindingFlags.Public | BindingFlags.Instance, [])!;
        private static readonly MethodInfo _clearHandlersGenericMethod = typeof(AsyncEventContainer).GetMethod(nameof(ClearHandlers), BindingFlags.Public | BindingFlags.Instance, [])!;

        protected readonly ConcurrentDictionary<Type, IAsyncEvent> _serverEvents = [];
        protected readonly ConcurrentDictionary<Type, ConcurrentDictionary<object, AsyncEventPriority>> _postHandlers = [];
        protected readonly ConcurrentDictionary<Type, ConcurrentDictionary<object, AsyncEventPriority>> _preHandlers = [];

        /// <inheritdoc />
        public IAsyncEvent<T> GetAsyncEvent<T>() where T : AsyncEventArgs
        {
            if (_serverEvents.TryGetValue(typeof(T), out IAsyncEvent? value))
            {
                return (IAsyncEvent<T>)value;
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

        public IAsyncEvent GetAsyncEvent(Type type)
        {
            ThrowIfNullOrNotAsyncEventArgs(type);
            MethodInfo genericMethod = _getAsyncEventGenericMethod.MakeGenericMethod(type);
            return (IAsyncEvent)genericMethod.Invoke(this, [])!;
        }

        /// <inheritdoc />
        public void Prepare<T>() where T : AsyncEventArgs
        {
            IAsyncEvent<T> asyncServerEvent = GetAsyncEvent<T>();
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

        public void Prepare(Type type)
        {
            ThrowIfNullOrNotAsyncEventArgs(type);
            MethodInfo genericMethod = _prepareGenericMethod.MakeGenericMethod(type);
            genericMethod.Invoke(this, []);
        }

        /// <inheritdoc />
        public void AddPreHandler<T>(AsyncEventPreHandler<T> preHandler, AsyncEventPriority priority) where T : AsyncEventArgs
        {
            ConcurrentDictionary<object, AsyncEventPriority> preHandlers = _preHandlers.GetOrAdd(typeof(T), []);
            preHandlers.AddOrUpdate(preHandler, priority, (_, _) => priority);
        }

        public void AddPreHandler(IAsyncEventPreHandler preHandler, AsyncEventPriority priority = AsyncEventPriority.Normal)
        {
            ThrowIfNullOrNotAsyncEventArgs(preHandler.EventArgsType);
            MethodInfo genericMethod = _addPreHandlerGenericMethod.MakeGenericMethod(preHandler.EventArgsType);
            genericMethod.Invoke(this, [(object)preHandler.PreInvokeAsync, priority]);
        }

        public void AddPreHandler<T>(IAsyncEventPreHandler<T> preHandler, AsyncEventPriority priority = AsyncEventPriority.Normal) where T : AsyncEventArgs
            => AddPreHandler<T>(preHandler.PreInvokeAsync, priority);

        /// <inheritdoc />
        public void AddPostHandler<T>(AsyncEventPostHandler<T> postHandler, AsyncEventPriority priority) where T : AsyncEventArgs
        {
            ConcurrentDictionary<object, AsyncEventPriority> postHandlers = _postHandlers.GetOrAdd(typeof(T), []);
            postHandlers.AddOrUpdate(postHandler, priority, (_, _) => priority);
        }

        public void AddPostHandler(IAsyncEventPostHandler postHandler, AsyncEventPriority priority = AsyncEventPriority.Normal)
        {
            ThrowIfNullOrNotAsyncEventArgs(postHandler.EventArgsType);
            MethodInfo genericMethod = _addPostHandlerGenericMethod.MakeGenericMethod(postHandler.EventArgsType);
            genericMethod.Invoke(this, [(object)postHandler.InvokeAsync, priority]);
        }

        public void AddPostHandler<T>(IAsyncEventPostHandler<T> postHandler, AsyncEventPriority priority = AsyncEventPriority.Normal) where T : AsyncEventArgs
            => AddPostHandler<T>(postHandler.InvokeAsync, priority);

        public void AddHandlers(object instance)
        {
            ArgumentNullException.ThrowIfNull(instance);
            if (instance is IAsyncEventPreHandler preHandler)
            {
                Delegate handler = preHandler.PreInvokeAsync;
                AddPreHandler(preHandler, handler.Method.GetCustomAttribute<AsyncEventHandlerAttribute>()?.Priority ?? AsyncEventPriority.Normal);
            }

            if (instance is IAsyncEventPostHandler postHandler)
            {
                Delegate handler = postHandler.InvokeAsync;
                AddPostHandler(postHandler, handler.Method.GetCustomAttribute<AsyncEventHandlerAttribute>()?.Priority ?? AsyncEventPriority.Normal);
            }
        }

        /// <inheritdoc />
        public void ClearPreHandlers<T>() where T : AsyncEventArgs => _preHandlers.Remove(typeof(T), out _);

        public void ClearPreHandlers(Type type)
        {
            ThrowIfNullOrNotAsyncEventArgs(type);
            MethodInfo genericMethod = _clearPreHandlersGenericMethod.MakeGenericMethod(type);
            genericMethod.Invoke(this, []);
        }

        /// <inheritdoc />
        public void ClearPreHandlers() => _preHandlers.Clear();

        /// <inheritdoc />
        public void ClearPostHandlers<T>() where T : AsyncEventArgs => _postHandlers.Remove(typeof(T), out _);

        public void ClearPostHandlers(Type type)
        {
            ThrowIfNullOrNotAsyncEventArgs(type);
            MethodInfo genericMethod = _clearPostHandlersGenericMethod.MakeGenericMethod(type);
            genericMethod.Invoke(this, []);
        }

        /// <inheritdoc />
        public void ClearPostHandlers() => _postHandlers.Clear();

        public void ClearHandlers<T>() where T : AsyncEventArgs
        {
            ClearPreHandlers<T>();
            ClearPostHandlers<T>();
        }

        public void ClearHandlers(Type type)
        {
            ThrowIfNullOrNotAsyncEventArgs(type);
            MethodInfo genericMethod = _clearHandlersGenericMethod.MakeGenericMethod(type);
            genericMethod.Invoke(this, []);
        }

        public void ClearHandlers()
        {
            ClearPreHandlers();
            ClearPostHandlers();
        }

        protected void ThrowIfNullOrNotAsyncEventArgs(Type type)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));
            if (type != typeof(AsyncEventArgs) && !type.IsSubclassOf(typeof(AsyncEventArgs)))
            {
                throw new ArgumentException($"Type must implement {nameof(AsyncEventArgs)}", nameof(type));
            }
        }
    }
}
