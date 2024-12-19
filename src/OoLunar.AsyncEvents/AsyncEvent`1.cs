using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    public sealed record AsyncEvent<TEventArgs> where TEventArgs : AsyncEventArgs
    {
        public IReadOnlyDictionary<AsyncEventPreHandler<TEventArgs>, AsyncEventPriority> PreHandlers => _preHandlers;
        public IReadOnlyDictionary<AsyncEventHandler<TEventArgs>, AsyncEventPriority> PostHandlers => _postHandlers;

        private readonly Dictionary<AsyncEventPreHandler<TEventArgs>, AsyncEventPriority> _preHandlers = [];
        private readonly Dictionary<AsyncEventHandler<TEventArgs>, AsyncEventPriority> _postHandlers = [];

        private readonly bool _parallelize;
        private readonly int _minimumParallelHandlers;

        private AsyncEventPreHandler<TEventArgs> _preEventHandlerDelegate;
        private AsyncEventHandler<TEventArgs> _postEventHandlerDelegate;

        public AsyncEvent() : this(false, 0) { }
        public AsyncEvent(bool parallelize) : this(parallelize, Environment.ProcessorCount) { }
        public AsyncEvent(bool parallelize, int minimumParallelHandlers)
        {
            _parallelize = parallelize;
            _minimumParallelHandlers = minimumParallelHandlers;
            _preEventHandlerDelegate = LazyPreHandler;
            _postEventHandlerDelegate = LazyPostHandler;
        }

        public void AddPreHandler(AsyncEventPreHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal) => _preHandlers.Add(handler, priority);
        public void AddPostHandler(AsyncEventHandler<TEventArgs> handler, AsyncEventPriority priority = AsyncEventPriority.Normal) => _postHandlers.Add(handler, priority);

        public bool RemovePreHandler(AsyncEventPreHandler<TEventArgs> handler) => _preHandlers.Remove(handler);
        public bool RemovePostHandler(AsyncEventHandler<TEventArgs> handler) => _postHandlers.Remove(handler);

        public async ValueTask<bool> InvokeAsync(TEventArgs eventArgs)
        {
            if (await InvokePreHandlersAsync(eventArgs))
            {
                await InvokePostHandlersAsync(eventArgs);
                return true;
            }

            return false;
        }

        public ValueTask<bool> InvokePreHandlersAsync(TEventArgs eventArgs) => _preEventHandlerDelegate(eventArgs);
        public ValueTask InvokePostHandlersAsync(TEventArgs eventArgs) => _postEventHandlerDelegate(eventArgs);

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
            else if (!_parallelize || preHandlers.Count < _minimumParallelHandlers)
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
            else if (!_parallelize || postHandlers.Count < _minimumParallelHandlers)
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

        public override string ToString() => $"{GetType()}, PreHandlers: {_preHandlers.Count}, PostHandlers: {_postHandlers.Count}";
    }
}
