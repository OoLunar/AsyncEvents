using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OoLunar.AsyncEvents
{
    /// <summary>
    /// An object that contains asynchronous events and event handlers.
    /// Intended to be used for dependency injection as a singleton or in a similar manner.
    /// </summary>
    public sealed class AsyncEventContainer
    {
        private readonly Dictionary<Type, object> _serverEvents = [];
        private readonly Dictionary<Type, List<(AsyncEventHandler, AsyncEventPriority)>> _postHandlers = [];
        private readonly Dictionary<Type, List<(AsyncEventPreHandler, AsyncEventPriority)>> _preHandlers = [];

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

        /// <summary>
        /// Registers an asynchronous event handler for the specified event type.
        /// </summary>
        /// <param name="preHandler">The asynchronous event handler to register.</param>
        /// <param name="priority">The priority of the event handler.</param>
        /// <typeparam name="T">The type of the asynchronous event arguments.</typeparam>
        public void AddPreHandler<T>(AsyncEventPreHandler<T> preHandler, AsyncEventPriority priority) where T : AsyncEventArgs
        {
            if (!_preHandlers.TryGetValue(typeof(T), out List<(AsyncEventPreHandler, AsyncEventPriority)>? preHandlers))
            {
                preHandlers = [];
                _preHandlers.Add(typeof(T), preHandlers);
            }

            preHandlers.Add((Unsafe.As<AsyncEventPreHandler<T>, AsyncEventPreHandler>(ref preHandler), priority));
        }

        /// <summary>
        /// Registers an asynchronous event handler for the specified event type.
        /// </summary>
        /// <param name="preHandler">The asynchronous event handler to register.</param>
        /// <param name="priority">The priority of the event handler.</param>
        /// <param name="type">The type of the asynchronous event arguments.</param>
        public void AddPreHandler(AsyncEventPreHandler preHandler, AsyncEventPriority priority, Type type)
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

        /// <summary>
        /// Registers an asynchronous event handler for the specified event type.
        /// </summary>
        /// <param name="postHandler">The asynchronous event handler to register.</param>
        /// <param name="priority">The priority of the event handler.</param>
        /// <typeparam name="T">The type of the asynchronous event arguments.</typeparam>
        public void AddPostHandler<T>(AsyncEventHandler<T> postHandler, AsyncEventPriority priority) where T : AsyncEventArgs
        {
            if (!_postHandlers.TryGetValue(typeof(T), out List<(AsyncEventHandler, AsyncEventPriority)>? postHandlers))
            {
                postHandlers = [];
                _postHandlers.Add(typeof(T), postHandlers);
            }

            postHandlers.Add((Unsafe.As<AsyncEventHandler<T>, AsyncEventHandler>(ref postHandler), priority));
        }

        /// <summary>
        /// Registers an asynchronous event handler for the specified event type.
        /// </summary>
        /// <param name="postHandler">The asynchronous event handler to register.</param>
        /// <param name="priority">The priority of the event handler.</param>
        /// <param name="type">The type of the asynchronous event arguments.</param>
        public void AddPostHandler(AsyncEventHandler postHandler, AsyncEventPriority priority, Type type)
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
