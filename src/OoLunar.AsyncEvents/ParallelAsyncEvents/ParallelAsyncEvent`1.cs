using System;
using System.Collections.Generic;
using OoLunar.AsyncEvents.AsyncEventClosures;

namespace OoLunar.AsyncEvents.ParallelAsyncEvents
{
    /// <summary>
    /// An asynchronous event that can be subscribed to and invoked.
    /// </summary>
    public class ParallelAsyncEvent<TEventArgs> : AsyncEvent<TEventArgs> where TEventArgs : AsyncEventArgs
    {
        /// <summary>
        /// When parallelizing, the minimum number of handlers required to start the parallelization process.
        /// </summary>
        /// <remarks>
        /// By default, parallelization will only occur when the number of handlers is equal to or greater than the number of logical processors on the current machine.
        /// </remarks>
        public int MinimumParallelHandlerCount { get; init; }

        /// <summary>
        /// Creates a new instance of <see cref="ParallelAsyncEvent{TEventArgs}"/> with the default parallelization settings.
        /// </summary>
        public ParallelAsyncEvent() : this(Environment.ProcessorCount) { }

        /// <summary>
        /// Creates a new instance of <see cref="ParallelAsyncEvent{TEventArgs}"/> with the specified parallelization settings.
        /// </summary>
        /// <param name="minimumParallelHandlers">
        /// The minimum number of handlers required to start the parallelization process.
        /// It is recommended to set this value to <see cref="Environment.ProcessorCount"/>, however fine-tuning may be required
        /// based on the number of handlers and the performance impact of parallelization.
        /// </param>
        public ParallelAsyncEvent(int minimumParallelHandlers) : base() => MinimumParallelHandlerCount = minimumParallelHandlers;

        /// <inheritdoc />
        protected override AsyncEventPreHandler<TEventArgs> CreatePreHandlerDelegate()
        {
            if (_preHandlers.Count == 0)
            {
                return EmptyPreHandler;
            }

            List<AsyncEventPreHandler<TEventArgs>> compiledHandlers = [];
            foreach (List<AsyncEventPreHandler<TEventArgs>> handlers in _preHandlers.Values)
            {
                compiledHandlers.Add(handlers.Count switch
                {
                    1 => handlers[0],
                    _ when handlers.Count >= MinimumParallelHandlerCount => new ParallelAsyncEventMultiPreHandler<TEventArgs>([.. handlers]).InvokeAsync,
                    2 => new AsyncEventTwoPreHandler<TEventArgs>(handlers[0], handlers[1]).InvokeAsync,
                    _ => new AsyncEventMultiPreHandler<TEventArgs>([.. handlers]).InvokeAsync,
                });
            }

            return compiledHandlers.Count switch
            {
                1 => compiledHandlers[0],
                2 => new AsyncEventTwoPreHandler<TEventArgs>(compiledHandlers[0], compiledHandlers[1]).InvokeAsync,
                _ => new AsyncEventMultiPreHandler<TEventArgs>([.. compiledHandlers]).InvokeAsync,
            };
        }

        /// <inheritdoc />
        protected override AsyncEventPostHandler<TEventArgs> CreatePostHandlerDelegate()
        {
            if (_postHandlers.Count == 0)
            {
                return EmptyPostHandler;
            }

            List<AsyncEventPostHandler<TEventArgs>> compiledHandlers = [];
            foreach (List<AsyncEventPostHandler<TEventArgs>> handlers in _postHandlers.Values)
            {
                compiledHandlers.Add(handlers.Count switch
                {
                    1 => handlers[0],
                    _ when handlers.Count > MinimumParallelHandlerCount => new ParallelAsyncEventMultiPostHandler<TEventArgs>([.. handlers]).InvokeAsync,
                    2 => new AsyncEventTwoPostHandler<TEventArgs>(handlers[0], handlers[1]).InvokeAsync,
                    _ => new AsyncEventMultiPostHandler<TEventArgs>([.. handlers]).InvokeAsync,
                });
            }

            return compiledHandlers.Count switch
            {
                1 => compiledHandlers[0],
                2 => new AsyncEventTwoPostHandler<TEventArgs>(compiledHandlers[0], compiledHandlers[1]).InvokeAsync,
                _ => new AsyncEventMultiPostHandler<TEventArgs>([.. compiledHandlers]).InvokeAsync,
            };
        }
    }
}
