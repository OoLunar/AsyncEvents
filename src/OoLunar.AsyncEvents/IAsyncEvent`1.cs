using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    /// <summary>
    /// A abstract interface for an asynchronous event.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    public interface IAsyncEvent<TEventArgs> : IAsyncEvent where TEventArgs : AsyncEventArgs
    {
        /// <summary>
        /// A read-only dictionary of currently registered pre-handlers and their priorities.
        /// </summary>
        public IReadOnlyDictionary<AsyncEventPriority, IReadOnlyList<AsyncEventPreHandler<TEventArgs>>> PreHandlers { get; }

        /// <summary>
        /// A read-only dictionary of currently registered post-handlers and their priorities.
        /// </summary>
        public IReadOnlyDictionary<AsyncEventPriority, IReadOnlyList<AsyncEventPostHandler<TEventArgs>>> PostHandlers { get; }

        /// <inheritdoc cref="AddPreHandler(AsyncEventPreHandler{TEventArgs}, AsyncEventPriority)" />
        public void AddPreHandler(IAsyncEventPreHandler handler, AsyncEventPriority priority = AsyncEventPriority.Normal)
        {
            if (handler is IAsyncEventPreHandler<TEventArgs> typedHandler)
            {
                AddPreHandler(typedHandler, priority);
            }
            else if (handler.EventArgsType == typeof(TEventArgs))
            {
                AddPreHandler(handler.PreInvokeAsync, priority);
            }
            else
            {
                throw new ArgumentException($"Handler type {handler.GetType()} does not match event type {typeof(TEventArgs)}.", nameof(handler));
            }
        }

        /// <inheritdoc cref="AddPreHandler(AsyncEventPreHandler{TEventArgs}, AsyncEventPriority)" />
        public void AddPreHandler(IAsyncEventPreHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal)
            => AddPreHandler(handler.PreInvokeAsync, priority);

        /// <inheritdoc cref="AddPostHandler(AsyncEventPostHandler{TEventArgs}, AsyncEventPriority)" />
        public void AddPostHandler(IAsyncEventPostHandler handler, AsyncEventPriority priority = AsyncEventPriority.Normal)
        {
            if (handler is IAsyncEventPostHandler<TEventArgs> typedHandler)
            {
                AddPostHandler(typedHandler, priority);
            }
            else if (handler.EventArgsType == typeof(TEventArgs))
            {
                AddPostHandler(handler.InvokeAsync, priority);
            }
            else
            {
                throw new ArgumentException($"Handler type {handler.GetType()} does not match event type {typeof(TEventArgs)}.", nameof(handler));
            }
        }

        /// <inheritdoc cref="AddPostHandler(AsyncEventPostHandler{TEventArgs}, AsyncEventPriority)" />
        public void AddPostHandler(IAsyncEventPostHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal)
            => AddPostHandler(handler.InvokeAsync, priority);

        /// <summary>
        /// Registers a new pre-handler with the specified priority.
        /// </summary>
        /// <remarks>
        /// If the handler is already registered, the priority will be updated.
        /// </remarks>
        /// <param name="handler">The pre-handler delegate to register.</param>
        /// <param name="priority">The priority of the handler. Handlers with higher priority will be invoked first.</param>
        public void AddPreHandler(AsyncEventPreHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal);

        /// <summary>
        /// Registers a new post-handler with the specified priority.
        /// </summary>
        /// <remarks>
        /// If the handler is already registered, the priority will be updated.
        /// </remarks>
        /// <param name="handler">The post-handler delegate to register.</param>
        /// <param name="priority">The priority of the handler. Handlers with higher priority will be invoked first.</param>
        public void AddPostHandler(AsyncEventPostHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal);

        /// <summary>
        /// Adds both pre-handlers and post-handlers to the event.
        /// </summary>
        /// <param name="instance">The object implementing <see cref="IAsyncEventPreHandler"/> and/or <see cref="IAsyncEventPostHandler"/>.</param>
        public void AddHandlers(object instance)
        {
            ArgumentNullException.ThrowIfNull(instance);
            if (instance is IAsyncEventPreHandler preHandler)
            {
                Delegate handler = preHandler.PreInvokeAsync;
                AddPreHandler(preHandler, handler.Method.GetCustomAttribute<AsyncEventHandlerPriorityAttribute>()?.Priority ?? AsyncEventPriority.Normal);
            }

            if (instance is IAsyncEventPostHandler postHandler)
            {
                Delegate handler = postHandler.InvokeAsync;
                AddPostHandler(postHandler, handler.Method.GetCustomAttribute<AsyncEventHandlerPriorityAttribute>()?.Priority ?? AsyncEventPriority.Normal);
            }
        }

        /// <summary>
        /// Removes a pre-handler from the event.
        /// </summary>
        /// <remarks>
        /// Warning: The handler will still be invoked until <see cref="IAsyncEvent.PrepareAsync(CancellationToken)"/> is called.
        /// </remarks>
        /// <param name="handler">The pre-handler delegate to remove.</param>
        /// <param name="priority">The priority of the handler to remove.</param>
        /// <returns><see langword="true"/> if the handler was successfully found and removed; otherwise, <see langword="false"/>.</returns>
        public bool RemovePreHandler(AsyncEventPreHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal);

        /// <summary>
        /// Removes a post-handler from the event.
        /// </summary>
        /// <remarks>
        /// Warning: The handler will still be invoked until <see cref="IAsyncEvent.PrepareAsync(CancellationToken)"/> is called.
        /// </remarks>
        /// <param name="handler">The post-handler delegate to remove.</param>
        /// <param name="priority">The priority of the handler to remove.</param>
        /// <returns><see langword="true"/> if the handler was successfully found and removed; otherwise, <see langword="false"/>.</returns>
        public bool RemovePostHandler(AsyncEventPostHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal);

        /// <summary>
        /// Invokes the pre-handlers with the specified event arguments.
        /// </summary>
        /// <remarks>
        /// Warning: Exceptions thrown by handlers will not be caught by this method. Any and all exception handling should be done by the caller.
        /// </remarks>
        /// <param name="eventArgs">The event arguments to pass to the handlers.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns><see langword="true"/> if all pre-handlers returned <see langword="true"/>; otherwise, <see langword="false"/>.</returns>
        public ValueTask<bool> InvokePreHandlersAsync(TEventArgs eventArgs, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invokes the post-handlers with the specified event arguments.
        /// </summary>
        /// <remarks>
        /// Warning: Exceptions thrown by handlers will not be caught by this method. Any and all exception handling should be done by the caller.
        /// </remarks>
        /// <param name="eventArgs">The event arguments to pass to the handlers.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        public ValueTask InvokePostHandlersAsync(TEventArgs eventArgs, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invokes the event with the specified event arguments.
        /// </summary>
        /// <remarks>
        /// This method invokes the pre-handlers first, then the post-handlers. If any pre-handler returns <see langword="false"/>, the post-handlers will not be invoked.
        /// <br/>
        /// Warning: Exceptions thrown by handlers will not be caught by this method. Any and all exception handling should be done by the caller.
        /// </remarks>
        /// <param name="eventArgs">The event arguments to pass to the handlers.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns><see langword="true"/> if all pre-handlers returned <see langword="true"/>; otherwise, <see langword="false"/>.</returns>
        public async ValueTask<bool> InvokeAsync(TEventArgs eventArgs, CancellationToken cancellationToken = default)
        {
            if (!await InvokePreHandlersAsync(eventArgs, cancellationToken))
            {
                return false;
            }

            await InvokePostHandlersAsync(eventArgs, cancellationToken);
            return true;
        }

        ValueTask<bool> IAsyncEvent.InvokePreHandlersAsync(AsyncEventArgs eventArgs, CancellationToken cancellationToken) => InvokePreHandlersAsync((TEventArgs)eventArgs, cancellationToken);

        ValueTask IAsyncEvent.InvokePostHandlersAsync(AsyncEventArgs eventArgs, CancellationToken cancellationToken) => InvokePostHandlersAsync((TEventArgs)eventArgs, cancellationToken);

        ValueTask<bool> IAsyncEvent.InvokeAsync(AsyncEventArgs eventArgs, CancellationToken cancellationToken) => InvokeAsync((TEventArgs)eventArgs, cancellationToken);
    }
}
