using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    /// <summary>
    /// An asynchronous event that can be subscribed to and invoked.
    /// </summary>
    [DebuggerDisplay("{ToString(),nq}")]
    public class AsyncEvent<TEventArgs> : IAsyncEvent<TEventArgs> where TEventArgs : AsyncEventArgs
    {
        /// <inheritdoc cref="AsyncEvent{TEventArgs}.PreHandlers" />
        public IReadOnlyDictionary<AsyncEventPriority, IReadOnlyList<AsyncEventPreHandler<TEventArgs>>> PreHandlers => TransformPreHandlers();

        /// <inheritdoc cref="AsyncEvent{TEventArgs}.PostHandlers" />
        public IReadOnlyDictionary<AsyncEventPriority, IReadOnlyList<AsyncEventPostHandler<TEventArgs>>> PostHandlers => TransformPostHandlers();

        /// <summary>
        /// A dictionary of currently registered pre-handlers and their priorities.
        /// </summary>
        protected readonly SortedList<AsyncEventPriority, List<AsyncEventPreHandler<TEventArgs>>> _preHandlers = [];

        /// <summary>
        /// A dictionary of currently registered post-handlers and their priorities.
        /// </summary>
        protected readonly SortedList<AsyncEventPriority, List<AsyncEventPostHandler<TEventArgs>>> _postHandlers = [];

        /// <summary>
        /// The delegate that contains the pre-handlers.
        /// </summary>
        protected AsyncEventPreHandler<TEventArgs> _preEventHandlerDelegate;

        /// <summary>
        /// The delegate that contains the post-handlers.
        /// </summary>
        protected AsyncEventPostHandler<TEventArgs> _postEventHandlerDelegate;

        /// <summary>
        /// A semaphore that ensures changes made to the handlers are thread-safe.
        /// </summary>
        protected SemaphoreSlim _semaphore = new(1, 1);

        /// <summary>
        /// Creates a new instance of <see cref="AsyncEvent{TEventArgs}"/>.
        /// </summary>
        public AsyncEvent()
        {
            _preEventHandlerDelegate = LazyPreHandler;
            _postEventHandlerDelegate = LazyPostHandler;
        }

        /// <inheritdoc />
        public void AddPreHandler(AsyncEventPreHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal)
        {
            _semaphore.Wait();
            try
            {
                if (!_preHandlers.TryGetValue(priority, out List<AsyncEventPreHandler<TEventArgs>>? handlers))
                {
                    handlers = [];
                    _preHandlers.Add(priority, handlers);
                }
                else if (handlers.Contains(handler))
                {
                    return;
                }

                handlers.Add(handler);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public void AddPostHandler(AsyncEventPostHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal)
        {
            _semaphore.Wait();
            try
            {
                if (!_postHandlers.TryGetValue(priority, out List<AsyncEventPostHandler<TEventArgs>>? handlers))
                {
                    handlers = [];
                    _postHandlers.Add(priority, handlers);
                }
                else if (handlers.Contains(handler))
                {
                    return;
                }

                handlers.Add(handler);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public bool RemovePreHandler(AsyncEventPreHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal)
        {
            _semaphore.Wait();
            try
            {
                if (!_preHandlers.TryGetValue(priority, out List<AsyncEventPreHandler<TEventArgs>>? handlers))
                {
                    return false;
                }
                else if (!handlers.Remove(handler))
                {
                    return false;
                }
                else if (handlers.Count == 0)
                {
                    _preHandlers.Remove(priority);
                }

                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public bool RemovePostHandler(AsyncEventPostHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal)
        {
            _semaphore.Wait();
            try
            {
                if (!_postHandlers.TryGetValue(priority, out List<AsyncEventPostHandler<TEventArgs>>? handlers))
                {
                    return false;
                }
                else if (!handlers.Remove(handler))
                {
                    return false;
                }
                else if (handlers.Count == 0)
                {
                    _postHandlers.Remove(priority);
                }

                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public void ClearPreHandlers()
        {
            _semaphore.Wait();
            try
            {
                _preHandlers.Clear();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public void ClearPostHandlers()
        {
            _semaphore.Wait();
            try
            {
                _postHandlers.Clear();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public async ValueTask<bool> InvokeAsync(TEventArgs eventArgs, CancellationToken cancellationToken = default)
        {
            if (await InvokePreHandlersAsync(eventArgs, cancellationToken))
            {
                await InvokePostHandlersAsync(eventArgs, cancellationToken);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public ValueTask<bool> InvokePreHandlersAsync(TEventArgs eventArgs, CancellationToken cancellationToken = default) => _preEventHandlerDelegate(eventArgs, cancellationToken);

        /// <inheritdoc />
        public ValueTask InvokePostHandlersAsync(TEventArgs eventArgs, CancellationToken cancellationToken = default) => _postEventHandlerDelegate(eventArgs, cancellationToken);

        /// <inheritdoc />
        public void Prepare()
        {
            _preEventHandlerDelegate = CreatePreHandlerDelegate();
            _postEventHandlerDelegate = CreatePostHandlerDelegate();
        }

        /// <inheritdoc />
        protected virtual AsyncEventPreHandler<TEventArgs> CreatePreHandlerDelegate()
        {
            if (_preHandlers.Count == 0)
            {
                return EmptyPreHandler;
            }

            // Aggregate all pre-handlers into a single array. Since the dictionary
            // is already presorted, we can just iterate over the values.
            List<AsyncEventPreHandler<TEventArgs>> compiledHandlers = [];
            foreach (List<AsyncEventPreHandler<TEventArgs>> handlers in _preHandlers.Values)
            {
                compiledHandlers.AddRange(handlers);
            }

            return compiledHandlers.Count switch
            {
                0 => EmptyPreHandler,
                1 => compiledHandlers[0],
                2 => new AsyncEventTwoPreHandler<TEventArgs>(compiledHandlers[0], compiledHandlers[1]).InvokeAsync,
                _ => new AsyncEventMultiPreHandler<TEventArgs>([.. compiledHandlers]).InvokeAsync,
            };
        }

        /// <inheritdoc />
        protected virtual AsyncEventPostHandler<TEventArgs> CreatePostHandlerDelegate()
        {
            if (_postHandlers.Count == 0)
            {
                return EmptyPostHandler;
            }

            // Aggregate all post-handlers into a single array. Since the dictionary
            // is already presorted, we can just iterate over the values.
            List<AsyncEventPostHandler<TEventArgs>> compiledHandlers = [];
            foreach (List<AsyncEventPostHandler<TEventArgs>> handlers in _postHandlers.Values)
            {
                compiledHandlers.AddRange(handlers);
            }

            return compiledHandlers.Count switch
            {
                0 => EmptyPostHandler,
                1 => compiledHandlers[0],
                2 => new AsyncEventTwoPostHandler<TEventArgs>(compiledHandlers[0], compiledHandlers[1]).InvokeAsync,
                _ => new AsyncEventMultiPostHandler<TEventArgs>([.. compiledHandlers]).InvokeAsync,
            };
        }

        /// <summary>
        /// An empty pre-handler that always returns <see langword="true"/>.
        /// </summary>
        protected static ValueTask<bool> EmptyPreHandler(TEventArgs _, CancellationToken __) => ValueTask.FromResult(true);

        /// <summary>
        /// An empty post-handler that does nothing.
        /// </summary>
        protected static ValueTask EmptyPostHandler(TEventArgs _, CancellationToken __) => ValueTask.CompletedTask;

        /// <summary>
        /// A lazy pre-handler that prepares the event before invoking the actual pre-handlers.
        /// This should only be called on the first unprepared invocation of the event, as it will
        /// replace itself through the preparation process.
        /// </summary>
        protected virtual ValueTask<bool> LazyPreHandler(TEventArgs eventArgs, CancellationToken cancellationToken)
        {
            Prepare();
            return _preEventHandlerDelegate(eventArgs, cancellationToken);
        }

        /// <summary>
        /// A lazy post-handler that prepares the event before invoking the actual post-handlers.
        /// This should only be called on the first unprepared invocation of the event, as it will
        /// replace itself through the preparation process.
        /// </summary>
        protected virtual ValueTask LazyPostHandler(TEventArgs eventArgs, CancellationToken cancellationToken)
        {
            Prepare();
            return _postEventHandlerDelegate(eventArgs, cancellationToken);
        }

        private Dictionary<AsyncEventPriority, IReadOnlyList<AsyncEventPreHandler<TEventArgs>>> TransformPreHandlers()
        {
            Dictionary<AsyncEventPriority, IReadOnlyList<AsyncEventPreHandler<TEventArgs>>> transformed = [];
            foreach (KeyValuePair<AsyncEventPriority, List<AsyncEventPreHandler<TEventArgs>>> pair in _preHandlers)
            {
                transformed.Add(pair.Key, pair.Value);
            }

            return transformed;
        }

        private Dictionary<AsyncEventPriority, IReadOnlyList<AsyncEventPostHandler<TEventArgs>>> TransformPostHandlers()
        {
            Dictionary<AsyncEventPriority, IReadOnlyList<AsyncEventPostHandler<TEventArgs>>> transformed = [];
            foreach (KeyValuePair<AsyncEventPriority, List<AsyncEventPostHandler<TEventArgs>>> pair in _postHandlers)
            {
                transformed.Add(pair.Key, pair.Value);
            }

            return transformed;
        }

        /// <inheritdoc/>
        public override string ToString() => $"{GetType().Name}, PreHandlers: {_preHandlers.Count}, PostHandlers: {_postHandlers.Count}";
    }
}
