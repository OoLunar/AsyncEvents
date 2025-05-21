using System;

namespace OoLunar.AsyncEvents
{
    public interface IAsyncEventContainer
    {
        /// <summary>
        /// Finds or lazily creates an asynchronous event of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the asynchronous event arguments.</typeparam>
        /// <returns>A prepared asynchronous event of the specified type with the appropriate handlers.</returns>
        public IAsyncEvent<T> GetAsyncEvent<T>() where T : AsyncEventArgs;

        /// <summary>
        /// Compiles the pre/post event handlers into a single delegate for faster execution. This should be called after any handlers have been modified.
        /// </summary>
        /// <typeparam name="T">The type of the asynchronous event arguments.</typeparam>
        public void Prepare<T>() where T : AsyncEventArgs;

        /// <summary>
        /// Registers an asynchronous event handler for the specified event type.
        /// </summary>
        /// <param name="preHandler">The asynchronous event handler to register.</param>
        /// <param name="priority">The priority of the event handler.</param>
        /// <typeparam name="T">The type of the asynchronous event arguments.</typeparam>
        public void AddPreHandler<T>(AsyncEventPreHandler<T> preHandler, AsyncEventPriority priority = AsyncEventPriority.Normal) where T : AsyncEventArgs;

        /// <summary>
        /// Registers an asynchronous event handler for the specified event type.
        /// </summary>
        /// <param name="postHandler">The asynchronous event handler to register.</param>
        /// <param name="priority">The priority of the event handler.</param>
        /// <typeparam name="T">The type of the asynchronous event arguments.</typeparam>
        public void AddPostHandler<T>(AsyncEventPostHandler<T> postHandler, AsyncEventPriority priority = AsyncEventPriority.Normal) where T : AsyncEventArgs;

        /// <summary>
        /// Removes all pre-event handlers for all event types.
        /// </summary>
        public void ClearPreHandlers();

        /// <summary>
        /// Removes all pre-event handlers for the specified event type.
        /// </summary>
        /// <typeparam name="T">The type of the asynchronous event arguments.</typeparam>
        public void ClearPreHandlers<T>() where T : AsyncEventArgs;

        /// <summary>
        /// Removes all post-event handlers for all event types.
        /// </summary>
        public void ClearPostHandlers();

        /// <summary>
        /// Removes all post-event handlers for the specified event type.
        /// </summary>
        /// <typeparam name="T">The type of the asynchronous event arguments.</typeparam>
        public void ClearPostHandlers<T>() where T : AsyncEventArgs;

        /// <summary>
        /// Clears all pre and post event handlers for the specified event type.
        /// </summary>
        /// <typeparam name="T">The type of the asynchronous event arguments.</typeparam>
        public void ClearHandlers<T>() where T : AsyncEventArgs;

        /// <summary>
        /// Clears all pre and post event handlers for all event types.
        /// </summary>
        public void ClearHandlers();

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
        public IAsyncEvent GetAsyncEvent(Type type);

        /// <summary>
        /// Compiles the pre/post event handlers into a single delegate for faster execution. This should be called after any handlers have been modified.
        /// </summary>
        /// <param name="type">The type of the asynchronous event arguments.</param>
        public void Prepare(Type type);

        public void AddPreHandler(IAsyncEventPreHandler preHandler, AsyncEventPriority priority = AsyncEventPriority.Normal);

        public void AddPreHandler<T>(IAsyncEventPreHandler<T> preHandler, AsyncEventPriority priority = AsyncEventPriority.Normal) where T : AsyncEventArgs;

        public void AddPostHandler(IAsyncEventPostHandler postHandler, AsyncEventPriority priority = AsyncEventPriority.Normal);

        public void AddPostHandler<T>(IAsyncEventPostHandler<T> postHandler, AsyncEventPriority priority = AsyncEventPriority.Normal) where T : AsyncEventArgs;

        public void AddHandlers(object instance);

        public void ClearPreHandlers(Type type);

        public void ClearPostHandlers(Type type);

        public void ClearHandlers(Type type);
    }
}
