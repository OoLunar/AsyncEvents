using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OoLunar.AsyncEvents.AsyncEventClosures;

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
        public IReadOnlyDictionary<AsyncEventPostHandler<TEventArgs>, AsyncEventPriority> PostHandlers => _postHandlers;

        private readonly Dictionary<AsyncEventPreHandler<TEventArgs>, AsyncEventPriority> _preHandlers = [];
        private readonly Dictionary<AsyncEventPostHandler<TEventArgs>, AsyncEventPriority> _postHandlers = [];

        private AsyncEventPreHandler<TEventArgs> _preEventHandlerDelegate;
        private AsyncEventPostHandler<TEventArgs> _postEventHandlerDelegate;

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
        public void AddPreHandler(AsyncEventPreHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal) => _preHandlers[handler] = priority;

        /// <summary>
        /// Registers a new post-handler with the specified priority.
        /// </summary>
        /// <remarks>
        /// If the handler is already registered, the priority will be updated.
        /// </remarks>
        /// <param name="handler">The post-handler delegate to register.</param>
        /// <param name="priority">The priority of the handler. Handlers with higher priority will be invoked first.</param>
        public void AddPostHandler(AsyncEventPostHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal) => _postHandlers[handler] = priority;

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
        public bool RemovePostHandler(AsyncEventPostHandler<TEventArgs> handler) => _postHandlers.Remove(handler);

        /// <summary>
        /// Removes all pre-handlers from the event.
        /// </summary>
        public void ClearPreHandlers() => _preHandlers.Clear();

        /// <summary>
        /// Removes all post-handlers from the event.
        /// </summary>
        public void ClearPostHandlers() => _postHandlers.Clear();

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
            _preEventHandlerDelegate = CreatePreHandlerDelegate();
            _postEventHandlerDelegate = CreatePostHandlerDelegate();
        }

        private AsyncEventPreHandler<TEventArgs> CreatePreHandlerDelegate()
        {
            if (_preHandlers.Count == 0)
            {
                return EmptyPreHandler;
            }
            else if (_preHandlers.Count == 1)
            {
                return _preHandlers.Keys.First();
            }

            AsyncEventPriority[] priorities = new AsyncEventPriority[_preHandlers.Count];
            AsyncEventPreHandler<TEventArgs>[] preHandlers = new AsyncEventPreHandler<TEventArgs>[_preHandlers.Count];

            _preHandlers.Values.CopyTo(priorities, 0);
            _preHandlers.Keys.CopyTo(preHandlers, 0);

            Array.Reverse(priorities);
            Array.Sort(priorities, preHandlers);

            return preHandlers.Length switch
            {
                2 => new AsyncEventTwoPreHandlerClosure<TEventArgs>(preHandlers[0], preHandlers[1]).InvokeAsync,
                _ when !ParallelizationEnabled || preHandlers.Length < MinimumParallelHandlerCount => new AsyncEventMultiPreHandlerClosure<TEventArgs>(preHandlers).InvokeAsync,
                _ => new AsyncEventParallelMultiPreHandlerClosure<TEventArgs>(preHandlers).InvokeAsync,
            };
        }

        private AsyncEventPostHandler<TEventArgs> CreatePostHandlerDelegate()
        {
            if (_postHandlers.Count == 0)
            {
                return EmptyPostHandler;
            }
            else if (_postHandlers.Count == 1)
            {
                return _postHandlers.Keys.First();
            }

            AsyncEventPriority[] priorities = new AsyncEventPriority[_postHandlers.Count];
            AsyncEventPostHandler<TEventArgs>[] postHandlers = new AsyncEventPostHandler<TEventArgs>[_postHandlers.Count];

            _postHandlers.Values.CopyTo(priorities, 0);
            _postHandlers.Keys.CopyTo(postHandlers, 0);

            Array.Reverse(priorities);
            Array.Sort(priorities, postHandlers);

            return postHandlers.Length switch
            {
                2 => new AsyncEventTwoPostHandlerClosure<TEventArgs>(postHandlers[0], postHandlers[1]).InvokeAsync,
                _ when !ParallelizationEnabled || postHandlers.Length < MinimumParallelHandlerCount => new AsyncEventMultiPostHandlerClosure<TEventArgs>(postHandlers).InvokeAsync,
                _ => new AsyncEventParallelMultiPostHandlerClosure<TEventArgs>(postHandlers).InvokeAsync,
            };
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
