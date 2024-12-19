using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    /// <summary>
    /// An asynchronous event that can be subscribed to and invoked.
    /// </summary>
    public sealed record AsyncEvent<TEventArgs> where TEventArgs : AsyncEventArgs
    {
        /// <summary>
        /// Whether to parallelize the invocation of handlers. If <see langword="true"/>, the event will invoke all handlers in parallel through <see cref="Parallel.ForEachAsync{TSource}(IAsyncEnumerable{TSource}, Func{TSource, System.Threading.CancellationToken, ValueTask})"/>.
        /// </summary>
        /// <remarks>
        /// Warning: Parallelization can cause handlers to be invoked out of order. If the order of handler invocation is important, do not enable parallelization.
        /// <br/>
        /// Warning: Parallelization has a notable performance impact when the number of handlers is small. It is recommended to only enable parallelization when the number of handlers is large.
        /// </remarks>
        public bool ParallelizationEnabled { get; init; }

        /// <summary>
        /// When parallelizing, the minimum number of handlers required to start the parallelization process.
        /// </summary>
        /// <remarks>
        /// By default, parallelization will only occur when the number of handlers is equal to or greater than the number of logical processors on the current machine.
        /// </remarks>
        public int MinimumParallelHandlerCount { get; init; }

        /// <summary>
        /// A read-only dictionary of currently registered pre-handlers and their priorities.
        /// </summary>
        public IReadOnlyDictionary<AsyncEventPreHandler<TEventArgs>, AsyncEventPriority> PreHandlers => _preHandlers;

        /// <summary>
        /// A read-only dictionary of currently registered post-handlers and their priorities.
        /// </summary>
        public IReadOnlyDictionary<AsyncEventHandler<TEventArgs>, AsyncEventPriority> PostHandlers => _postHandlers;

        private readonly Dictionary<AsyncEventPreHandler<TEventArgs>, AsyncEventPriority> _preHandlers = [];
        private readonly Dictionary<AsyncEventHandler<TEventArgs>, AsyncEventPriority> _postHandlers = [];

        private AsyncEventPreHandler<TEventArgs> _preEventHandlerDelegate;
        private AsyncEventHandler<TEventArgs> _postEventHandlerDelegate;

        /// <summary>
        /// Creates a new instance of <see cref="AsyncEvent{TEventArgs}"/> with parallelization disabled.
        /// </summary>
        public AsyncEvent() : this(false, 0) { }

        /// <summary>
        /// Creates a new instance of <see cref="AsyncEvent{TEventArgs}"/> with the specified parallelization settings.
        /// </summary>
        /// <param name="parallelize">Whether to parallelize the invocation of handlers. If <see langword="true"/>, <c>MinimumParallelHandlerCount</c> will be set to <see cref="Environment.ProcessorCount"/>.</param>
        public AsyncEvent(bool parallelize) : this(parallelize, Environment.ProcessorCount) { }

        /// <summary>
        /// Creates a new instance of <see cref="AsyncEvent{TEventArgs}"/> with the specified parallelization settings.
        /// </summary>
        /// <param name="parallelize">Whether to parallelize the invocation of handlers.</param>
        /// <param name="minimumParallelHandlers">The minimum number of handlers required to start the parallelization process. It is recommended to set this value to <see cref="Environment.ProcessorCount"/>, however fine-tuning may be required based on the number of handlers and the performance impact of parallelization.</param>
        public AsyncEvent(bool parallelize, int minimumParallelHandlers)
        {
            ParallelizationEnabled = parallelize;
            MinimumParallelHandlerCount = minimumParallelHandlers;
            _preEventHandlerDelegate = LazyPreHandler;
            _postEventHandlerDelegate = LazyPostHandler;
        }

        /// <summary>
        /// Registers a new pre-handler with the specified priority.
        /// </summary>
        /// <remarks>
        /// If the handler is already registered, the priority will be updated.
        /// </remarks>
        /// <param name="handler">The pre-handler delegate to register.</param>
        /// <param name="priority">The priority of the handler. Handlers with higher priority will be invoked first.</param>
        public void AddPreHandler(AsyncEventPreHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal) => _preHandlers[handler] = priority;

        /// <summary>
        /// Registers a new post-handler with the specified priority.
        /// </summary>
        /// <remarks>
        /// If the handler is already registered, the priority will be updated.
        /// </remarks>
        /// <param name="handler">The post-handler delegate to register.</param>
        /// <param name="priority">The priority of the handler. Handlers with higher priority will be invoked first.</param>
        public void AddPostHandler(AsyncEventHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal) => _postHandlers[handler] = priority;

        /// <summary>
        /// Removes a pre-handler from the event.
        /// </summary>
        /// <remarks>
        /// Warning: The handler will still be invoked until <see cref="Prepare"/> is called.
        /// </remarks>
        /// <param name="handler">The pre-handler delegate to remove.</param>
        /// <returns><see langword="true"/> if the handler was successfully found and removed; otherwise, <see langword="false"/>.</returns>
        public bool RemovePreHandler(AsyncEventPreHandler<TEventArgs> handler) => _preHandlers.Remove(handler);

        /// <summary>
        /// Removes a post-handler from the event.
        /// </summary>
        /// <remarks>
        /// Warning: The handler will still be invoked until <see cref="Prepare"/> is called.
        /// </remarks>
        /// <param name="handler">The post-handler delegate to remove.</param>
        /// <returns><see langword="true"/> if the handler was successfully found and removed; otherwise, <see langword="false"/>.</returns>
        public bool RemovePostHandler(AsyncEventHandler<TEventArgs> handler) => _postHandlers.Remove(handler);

        /// <summary>
        /// Invokes the event with the specified event arguments.
        /// </summary>
        /// <remarks>
        /// This method invokes the pre-handlers first, then the post-handlers. If any pre-handler returns <see langword="false"/>, the post-handlers will not be invoked.
        /// <br/>
        /// Warning: Exceptions thrown by handlers will not be caught by this method. Any and all exception handling should be done by the caller.
        /// </remarks>
        /// <param name="eventArgs">The event arguments to pass to the handlers.</param>
        /// <returns><see langword="true"/> if all pre-handlers returned <see langword="true"/>; otherwise, <see langword="false"/>.</returns>
        public async ValueTask<bool> InvokeAsync(TEventArgs eventArgs)
        {
            if (await InvokePreHandlersAsync(eventArgs))
            {
                await InvokePostHandlersAsync(eventArgs);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Invokes the pre-handlers with the specified event arguments.
        /// </summary>
        /// <remarks>
        /// Warning: Exceptions thrown by handlers will not be caught by this method. Any and all exception handling should be done by the caller.
        /// </remarks>
        /// <param name="eventArgs">The event arguments to pass to the handlers.</param>
        /// <returns><see langword="true"/> if all pre-handlers returned <see langword="true"/>; otherwise, <see langword="false"/>.</returns>
        public ValueTask<bool> InvokePreHandlersAsync(TEventArgs eventArgs) => _preEventHandlerDelegate(eventArgs);

        /// <summary>
        /// Invokes the post-handlers with the specified event arguments.
        /// </summary>
        /// <remarks>
        /// Warning: Exceptions thrown by handlers will not be caught by this method. Any and all exception handling should be done by the caller.
        /// </remarks>
        /// <param name="eventArgs">The event arguments to pass to the handlers.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        public ValueTask InvokePostHandlersAsync(TEventArgs eventArgs) => _postEventHandlerDelegate(eventArgs);

        /// <summary>
        /// Efficiently prepares the event for invocation by compiling the pre/post-handlers into a single delegate.
        /// </summary>
        [SuppressMessage("Roslyn", "IDE0045", Justification = "Ternary rabbit hole.")]
        public void Prepare()
        {
            List<AsyncEventPreHandler<TEventArgs>> preHandlers = _preHandlers.OrderBy(x => x.Value).Select(x => x.Key).ToList();
            List<AsyncEventHandler<TEventArgs>> postHandlers = _postHandlers.OrderBy(x => x.Value).Select(x => x.Key).ToList();

            if (preHandlers.Count == 0)
            {
                _preEventHandlerDelegate = EmptyPreHandler;
            }
            else if (preHandlers.Count == 1)
            {
                _preEventHandlerDelegate = preHandlers[0];
            }
            else if (preHandlers.Count == 2)
            {
                _preEventHandlerDelegate = async ValueTask<bool> (TEventArgs eventArgs) => await preHandlers[0](eventArgs) && await preHandlers[1](eventArgs);
            }
            else if (!ParallelizationEnabled || preHandlers.Count < MinimumParallelHandlerCount)
            {
                _preEventHandlerDelegate = async eventArgs =>
                {
                    bool result = true;
                    foreach (AsyncEventPreHandler<TEventArgs> handler in preHandlers)
                    {
                        result &= await handler(eventArgs);
                    }

                    return result;
                };
            }
            else
            {
                _preEventHandlerDelegate = async (TEventArgs eventArgs) =>
                {
                    bool result = true;
                    await Parallel.ForEachAsync(preHandlers, async (handler, cancellationToken) => result &= await handler(eventArgs));
                    return result;
                };
            }

            if (postHandlers.Count == 0)
            {
                _postEventHandlerDelegate = EmptyPostHandler;
            }
            else if (postHandlers.Count == 1)
            {
                _postEventHandlerDelegate = postHandlers[0];
            }
            else if (!ParallelizationEnabled || postHandlers.Count < MinimumParallelHandlerCount)
            {
                _postEventHandlerDelegate = async (TEventArgs eventArgs) =>
                {
                    foreach (AsyncEventHandler<TEventArgs> handler in postHandlers)
                    {
                        await handler(eventArgs);
                    }
                };
            }
            else
            {
                _postEventHandlerDelegate = async (TEventArgs eventArgs) =>
                    await Parallel.ForEachAsync(postHandlers, async (handler, cancellationToken) => await handler(eventArgs));
            }
        }

        private static ValueTask<bool> EmptyPreHandler(TEventArgs _) => ValueTask.FromResult(true);
        private static ValueTask EmptyPostHandler(TEventArgs _) => ValueTask.CompletedTask;

        private ValueTask<bool> LazyPreHandler(TEventArgs eventArgs)
        {
            Prepare();
            return _preEventHandlerDelegate(eventArgs);
        }

        private ValueTask LazyPostHandler(TEventArgs eventArgs)
        {
            Prepare();
            return _postEventHandlerDelegate(eventArgs);
        }

        /// <inheritdoc/>
        public override string ToString() => $"{GetType()}, PreHandlers: {_preHandlers.Count}, PostHandlers: {_postHandlers.Count}";
    }
}
