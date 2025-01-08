using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OoLunar.AsyncEvents.DebugAsyncEvents
{
    /// <summary>
    /// An asynchronous event type that logs any changes to the event.
    /// </summary>
    public class DebugAsyncEvent<TEventArgs> : IAsyncEvent<TEventArgs> where TEventArgs : AsyncEventArgs
    {
        /// <inheritdoc />
        public IReadOnlyDictionary<AsyncEventPriority, IReadOnlyList<AsyncEventPreHandler<TEventArgs>>> PreHandlers => _asyncEvent.PreHandlers;

        /// <inheritdoc />
        public IReadOnlyDictionary<AsyncEventPriority, IReadOnlyList<AsyncEventPostHandler<TEventArgs>>> PostHandlers => _asyncEvent.PostHandlers;

        /// <summary>
        /// The asynchronous event that is being debugged.
        /// </summary>
        protected readonly IAsyncEvent<TEventArgs> _asyncEvent;

        /// <summary>
        /// The logger that is used to log any changes to the event.
        /// </summary>
        protected readonly ILogger<DebugAsyncEvent<TEventArgs>> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="DebugAsyncEvent{TEventArgs}"/>, which logs any changes to the underlying event.
        /// </summary>
        /// <param name="asyncEvent">The asynchronous event that is being debugged.</param>
        /// <param name="logger">The logger that is used to log any changes to the event.</param>
        public DebugAsyncEvent(IAsyncEvent<TEventArgs> asyncEvent, ILogger<DebugAsyncEvent<TEventArgs>> logger)
        {
            _asyncEvent = asyncEvent;
            _logger = logger;
        }

        /// <inheritdoc />
        public void AddPostHandler(AsyncEventPostHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal)
        {
            _logger.LogDebug("Adding post-handler '{Handler}' with priority {Priority}.", handler, priority);
            if (TryFindPostHandler(handler, priority, out _))
            {
                _logger.LogDebug("Post-handler '{Handler}' with priority {Priority} already exists.", handler, priority);
                return;
            }

            _asyncEvent.AddPostHandler(new DebugPostHandlerWrapper<TEventArgs>(handler, _logger).StartPostHandlerAsync, priority);
            _logger.LogDebug("Added post-handler '{Handler}' with priority {Priority}.", handler, priority);
        }

        /// <inheritdoc />
        public void AddPreHandler(AsyncEventPreHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal)
        {
            _logger.LogDebug("Adding pre-handler '{Handler}' with priority {Priority}.", handler, priority);
            if (TryFindPreHandler(handler, priority, out _))
            {
                _logger.LogDebug("Pre-handler '{Handler}' with priority {Priority} already exists.", handler, priority);
                return;
            }

            _asyncEvent.AddPreHandler(new DebugPreHandlerWrapper<TEventArgs>(handler, _logger).StartPreHandlerAsync, priority);
            _logger.LogDebug("Added pre-handler '{Handler}' with priority {Priority}.", handler, priority);
        }

        /// <inheritdoc />
        public void ClearPostHandlers()
        {
            _logger.LogDebug("Clearing all {Count} post-handlers.", _asyncEvent.PostHandlers.Count);
            _asyncEvent.ClearPostHandlers();
            _logger.LogDebug("Cleared all {Count} post-handlers.", _asyncEvent.PostHandlers.Count);
        }

        /// <inheritdoc />
        public void ClearPreHandlers()
        {
            _logger.LogDebug("Clearing all {Count} pre-handlers.", _asyncEvent.PreHandlers.Count);
            _asyncEvent.ClearPreHandlers();
            _logger.LogDebug("Cleared all {Count} pre-handlers.", _asyncEvent.PreHandlers.Count);
        }

        /// <inheritdoc />
        public async ValueTask InvokePostHandlersAsync(TEventArgs eventArgs)
        {
            _logger.LogDebug("Invoking all {Count} post-handlers.", _asyncEvent.PostHandlers.Count);
            await _asyncEvent.InvokePostHandlersAsync(eventArgs);
            _logger.LogDebug("Invoked all {Count} post-handlers.", _asyncEvent.PostHandlers.Count);
        }

        /// <inheritdoc />
        public async ValueTask<bool> InvokePreHandlersAsync(TEventArgs eventArgs)
        {
            _logger.LogDebug("Invoking all {Count} pre-handlers.", _asyncEvent.PreHandlers.Count);
            bool result = await _asyncEvent.InvokePreHandlersAsync(eventArgs);
            _logger.LogDebug("Invoked all {Count} pre-handlers.", _asyncEvent.PreHandlers.Count);
            return result;
        }

        /// <inheritdoc />
        public void Prepare()
        {
            _logger.LogDebug("Preparing event.");
            List<DebugClosure<TEventArgs>> closures = [];
            foreach (KeyValuePair<AsyncEventPriority, IReadOnlyList<AsyncEventPreHandler<TEventArgs>>> preHandlers in _asyncEvent.PreHandlers)
            {
                foreach (AsyncEventPreHandler<TEventArgs> handler in preHandlers.Value)
                {
                    _logger.LogDebug("Priority {Priority} found pre-handler '{Handler}'", preHandlers.Key, handler);
                }

                DebugClosure<TEventArgs> closure = new(preHandlers.Key, _logger);
                closures.Add(closure);
                AddPreHandler(closure.StartPreHandlerAsync, preHandlers.Key);
            }

            foreach (KeyValuePair<AsyncEventPriority, IReadOnlyList<AsyncEventPostHandler<TEventArgs>>> postHandlers in _asyncEvent.PostHandlers)
            {
                foreach (AsyncEventPostHandler<TEventArgs> handler in postHandlers.Value)
                {
                    _logger.LogDebug("Priority {Priority} found post-handler '{Handler}'", postHandlers.Key, handler);
                }

                DebugClosure<TEventArgs> closure = new(postHandlers.Key, _logger);
                closures.Add(closure);
                AddPostHandler(closure.StartPostHandlerAsync, postHandlers.Key);
            }

            _asyncEvent.Prepare();

            foreach (DebugClosure<TEventArgs> closure in closures)
            {
                if (PreHandlers.TryGetValue(closure.Priority, out IReadOnlyList<AsyncEventPreHandler<TEventArgs>>? preHandlers))
                {
                    RemovePreHandler(closure.StartPreHandlerAsync, closure.Priority);
                    _logger.LogDebug("Pre-handler closure for {Priority} priority has {Count} pre-handlers.", closure.Priority, preHandlers.Count);
                }

                if (PostHandlers.TryGetValue(closure.Priority, out IReadOnlyList<AsyncEventPostHandler<TEventArgs>>? postHandlers))
                {
                    RemovePostHandler(closure.StartPostHandlerAsync, closure.Priority);
                    _logger.LogDebug("Post-handler closure for {Priority} priority has {Count} post-handlers.", closure.Priority, postHandlers.Count);
                }
            }

            _logger.LogDebug("Prepared event.");
        }

        /// <inheritdoc />
        public bool RemovePostHandler(AsyncEventPostHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal)
        {
            _logger.LogDebug("Removing post-handler '{Handler}' with priority {Priority}.", handler, priority);
            if (!TryFindPostHandler(handler, priority, out DebugPostHandlerWrapper<TEventArgs>? wrapper))
            {
                _logger.LogDebug("Post-handler '{Handler}' with priority {Priority} not found.", handler, priority);
                return false;
            }
            else if (!_asyncEvent.RemovePostHandler(wrapper.StartPostHandlerAsync, priority))
            {
                throw new UnreachableException("Post-handler was found, but could not be removed.");
            }

            _logger.LogDebug("Removed post-handler '{Handler}' with priority {Priority}.", handler, priority);
            return true;
        }

        /// <inheritdoc />
        public bool RemovePreHandler(AsyncEventPreHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal)
        {
            _logger.LogDebug("Removing pre-handler '{Handler}' with priority {Priority}.", handler, priority);
            if (!TryFindPreHandler(handler, priority, out DebugPreHandlerWrapper<TEventArgs>? wrapper))
            {
                _logger.LogDebug("Pre-handler '{Handler}' with priority {Priority} not found.", handler, priority);
                return false;
            }
            else if (!_asyncEvent.RemovePreHandler(wrapper.StartPreHandlerAsync, priority))
            {
                throw new UnreachableException("Pre-handler was found, but could not be removed.");
            }

            _logger.LogDebug("Removed pre-handler '{Handler}' with priority {Priority}.", handler, priority);
            return true;
        }

        private bool TryFindPreHandler(AsyncEventPreHandler<TEventArgs> handler, AsyncEventPriority priority, [NotNullWhen(true)] out DebugPreHandlerWrapper<TEventArgs>? wrapper)
        {
            if (!_asyncEvent.PreHandlers.TryGetValue(priority, out IReadOnlyList<AsyncEventPreHandler<TEventArgs>>? handlers))
            {
                _logger.LogDebug("Pre-handler '{Handler}' with priority {Priority} not found.", handler, priority);
                wrapper = null;
                return false;
            }

            foreach (AsyncEventPreHandler<TEventArgs> currentHandler in handlers)
            {
                if (currentHandler.Target is not DebugPreHandlerWrapper<TEventArgs> currentWrapper || currentWrapper.PreHandler != handler)
                {
                    continue;
                }

                _logger.LogDebug("Found pre-handler '{Handler}' with priority {Priority}.", handler, priority);
                wrapper = currentWrapper;
                return true;
            }

            _logger.LogDebug("Pre-handler '{Handler}' with priority {Priority} not found.", handler, priority);
            wrapper = null;
            return false;
        }

        private bool TryFindPostHandler(AsyncEventPostHandler<TEventArgs> handler, AsyncEventPriority priority, [NotNullWhen(true)] out DebugPostHandlerWrapper<TEventArgs>? wrapper)
        {
            if (!_asyncEvent.PostHandlers.TryGetValue(priority, out IReadOnlyList<AsyncEventPostHandler<TEventArgs>>? handlers))
            {
                _logger.LogDebug("Post-handler '{Handler}' with priority {Priority} not found.", handler, priority);
                wrapper = null;
                return false;
            }

            foreach (AsyncEventPostHandler<TEventArgs> currentHandler in handlers)
            {
                if (currentHandler.Target is not DebugPostHandlerWrapper<TEventArgs> currentWrapper || currentWrapper.PostHandler != handler)
                {
                    continue;
                }

                _logger.LogDebug("Found post-handler '{Handler}' with priority {Priority}.", handler, priority);
                wrapper = currentWrapper;
                return true;
            }

            _logger.LogDebug("Post-handler '{Handler}' with priority {Priority} not found.", handler, priority);
            wrapper = null;
            return false;
        }
    }
}
