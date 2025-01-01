using System;
using System.Collections.Generic;
using System.Reflection;
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

        /// <summary>
        /// Searches the specified type for methods with the <see cref="AsyncEventHandlerAttribute"/> and registers them as pre/post-handlers.
        /// </summary>
        /// <param name="target">The target object to bind the methods to. If <see langword="null"/>, only static methods will be registered. If not <see langword="null"/>, the object will be reused when binding to the methods.</param>
        /// <typeparam name="T">The type to search for methods marked with the <see cref="AsyncEventHandlerAttribute"/>.</typeparam>
        public void AddHandlers<T>(object? target = null) => AddHandlers(typeof(T), target);

        /// <summary>
        /// Searches the specified type for methods with the <see cref="AsyncEventHandlerAttribute"/> and registers them as pre/post-handlers.
        /// </summary>
        /// <param name="type">The type to search for methods marked with the <see cref="AsyncEventHandlerAttribute"/>.</param>
        /// <param name="target">The target object to bind the methods to. If <see langword="null"/>, only static methods will be registered. If not <see langword="null"/>, the object will be reused when binding to the methods.</param>
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
                if (parameters.Length != 1 || !parameters[0].ParameterType.IsAssignableTo(typeof(TEventArgs)))
                {
                    continue;
                }

                if (method.ReturnType == typeof(ValueTask<bool>))
                {
                    if (method.IsStatic)
                    {
                        AddPreHandler((AsyncEventPreHandler<TEventArgs>)Delegate.CreateDelegate(typeof(AsyncEventPreHandler<TEventArgs>), method), attribute.Priority);
                    }
                    else if (target is not null)
                    {
                        AddPreHandler((AsyncEventPreHandler<TEventArgs>)Delegate.CreateDelegate(typeof(AsyncEventPreHandler<TEventArgs>), target, method), attribute.Priority);
                    }
                }
                else
                {
                    if (method.IsStatic)
                    {
                        AddPostHandler((AsyncEventPostHandler<TEventArgs>)Delegate.CreateDelegate(typeof(AsyncEventPostHandler<TEventArgs>), method), attribute.Priority);
                    }
                    else if (target is not null)
                    {
                        AddPostHandler((AsyncEventPostHandler<TEventArgs>)Delegate.CreateDelegate(typeof(AsyncEventPostHandler<TEventArgs>), target, method), attribute.Priority);
                    }
                }
            }
        }

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
        /// Removes a pre-handler from the event.
        /// </summary>
        /// <remarks>
        /// Warning: The handler will still be invoked until <see cref="IAsyncEvent.PrepareAsync()"/> is called.
        /// </remarks>
        /// <param name="handler">The pre-handler delegate to remove.</param>
        /// <param name="priority">The priority of the handler to remove.</param>
        /// <returns><see langword="true"/> if the handler was successfully found and removed; otherwise, <see langword="false"/>.</returns>
        public bool RemovePreHandler(AsyncEventPreHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal);

        /// <summary>
        /// Removes a post-handler from the event.
        /// </summary>
        /// <remarks>
        /// Warning: The handler will still be invoked until <see cref="IAsyncEvent.PrepareAsync()"/> is called.
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
        /// <returns><see langword="true"/> if all pre-handlers returned <see langword="true"/>; otherwise, <see langword="false"/>.</returns>
        public ValueTask<bool> InvokePreHandlersAsync(TEventArgs eventArgs);

        /// <summary>
        /// Invokes the post-handlers with the specified event arguments.
        /// </summary>
        /// <remarks>
        /// Warning: Exceptions thrown by handlers will not be caught by this method. Any and all exception handling should be done by the caller.
        /// </remarks>
        /// <param name="eventArgs">The event arguments to pass to the handlers.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        public ValueTask InvokePostHandlersAsync(TEventArgs eventArgs);

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
            if (!await InvokePreHandlersAsync(eventArgs))
            {
                return false;
            }

            await InvokePostHandlersAsync(eventArgs);
            return true;
        }

        ValueTask<bool> IAsyncEvent.InvokePreHandlersAsync(AsyncEventArgs eventArgs) => InvokePreHandlersAsync((TEventArgs)eventArgs);

        ValueTask IAsyncEvent.InvokePostHandlersAsync(AsyncEventArgs eventArgs) => InvokePostHandlersAsync((TEventArgs)eventArgs);

        ValueTask<bool> IAsyncEvent.InvokeAsync(AsyncEventArgs eventArgs) => InvokeAsync((TEventArgs)eventArgs);
    }
}
