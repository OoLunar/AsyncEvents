using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OoLunar.AsyncEvents
{
    public sealed class AsyncEventContainer
    {
        private readonly Dictionary<Type, object> _serverEvents = [];
        private readonly Dictionary<Type, List<(AsyncEventHandler, AsyncEventPriority)>> _postHandlers = [];
        private readonly Dictionary<Type, List<(AsyncEventPreHandler, AsyncEventPriority)>> _preHandlers = [];

        private readonly bool _parallelize;
        private readonly int _minimumParallelHandlers;

        public AsyncEventContainer() : this(false, 0) { }
        public AsyncEventContainer(bool parallelize) : this(parallelize, Environment.ProcessorCount) { }
        public AsyncEventContainer(bool parallelize, int minimumParallelHandlers)
        {
            _parallelize = parallelize;
            _minimumParallelHandlers = minimumParallelHandlers;
        }

        public AsyncEvent<T> GetAsyncServerEvent<T>() where T : AsyncEventArgs
        {
            if (_serverEvents.TryGetValue(typeof(T), out object? value))
            {
                return (AsyncEvent<T>)value;
            }

            AsyncEvent<T> asyncServerEvent = new(_parallelize, _minimumParallelHandlers);
            if (_preHandlers.TryGetValue(typeof(T), out List<(AsyncEventPreHandler, AsyncEventPriority)>? preHandlers))
            {
                foreach ((AsyncEventPreHandler preHandler, AsyncEventPriority priority) in preHandlers)
                {
                    // Cannot use 'preHandler' as a ref or out value because it is a 'foreach iteration variable' csharp(CS1657)
                    AsyncEventPreHandler localPreHandler = preHandler;
                    asyncServerEvent.AddPreHandler(Unsafe.As<AsyncEventPreHandler, AsyncEventPreHandler<T>>(ref localPreHandler), priority);
                }
            }

            if (_postHandlers.TryGetValue(typeof(T), out List<(AsyncEventHandler, AsyncEventPriority)>? postHandlers))
            {
                foreach ((AsyncEventHandler postHandler, AsyncEventPriority priority) in postHandlers)
                {
                    // Cannot use 'postHandler' as a ref or out value because it is a 'foreach iteration variable' csharp(CS1657)
                    AsyncEventHandler localPostHandler = postHandler;
                    asyncServerEvent.AddPostHandler(Unsafe.As<AsyncEventHandler, AsyncEventHandler<T>>(ref localPostHandler), priority);
                }
            }

            asyncServerEvent.Prepare();
            _serverEvents.Add(typeof(T), asyncServerEvent);
            return asyncServerEvent;
        }

        public void AddPreHandler<T>(AsyncEventPreHandler<T> preHandler, AsyncEventPriority priority) where T : AsyncEventArgs
        {
            if (!_preHandlers.TryGetValue(typeof(T), out List<(AsyncEventPreHandler, AsyncEventPriority)>? preHandlers))
            {
                preHandlers = [];
                _preHandlers.Add(typeof(T), preHandlers);
            }

            preHandlers.Add((Unsafe.As<AsyncEventPreHandler<T>, AsyncEventPreHandler>(ref preHandler), priority));
        }

        public void AddPreHandler(Type type, AsyncEventPreHandler preHandler, AsyncEventPriority priority)
        {
            if (type.IsAssignableFrom(typeof(AsyncEventArgs)))
            {
                throw new ArgumentException("Type must be a subclass of AsyncServerEventArgs", nameof(type));
            }

            if (!_preHandlers.TryGetValue(type, out List<(AsyncEventPreHandler, AsyncEventPriority)>? preHandlers))
            {
                preHandlers = [];
                _preHandlers.Add(type, preHandlers);
            }

            preHandlers.Add((preHandler, priority));
        }

        public void AddPostHandler<T>(AsyncEventHandler<T> postHandler, AsyncEventPriority priority) where T : AsyncEventArgs
        {
            if (!_postHandlers.TryGetValue(typeof(T), out List<(AsyncEventHandler, AsyncEventPriority)>? postHandlers))
            {
                postHandlers = [];
                _postHandlers.Add(typeof(T), postHandlers);
            }

            postHandlers.Add((Unsafe.As<AsyncEventHandler<T>, AsyncEventHandler>(ref postHandler), priority));
        }

        public void AddPostHandler(Type type, AsyncEventHandler postHandler, AsyncEventPriority priority)
        {
            if (type.IsAssignableFrom(typeof(AsyncEventArgs)))
            {
                throw new ArgumentException("Type must be a subclass of AsyncServerEventArgs", nameof(type));
            }

            if (!_postHandlers.TryGetValue(type, out List<(AsyncEventHandler, AsyncEventPriority)>? postHandlers))
            {
                postHandlers = [];
                _postHandlers.Add(type, postHandlers);
            }

            postHandlers.Add((postHandler, priority));
        }
    }
}
