using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.ParallelAsyncEvents
{
    internal class ParallelAsyncEventMultiPostHandlerClosure<TAsyncEventArgs> where TAsyncEventArgs : AsyncEventArgs
    {
        private readonly AsyncEventPostHandler<TAsyncEventArgs>[] _handlers;

        public ParallelAsyncEventMultiPostHandlerClosure(AsyncEventPostHandler<TAsyncEventArgs>[] handlers) => _handlers = handlers;

        public async ValueTask InvokeAsync(TAsyncEventArgs eventArgs)
        {
            List<Exception>? errors = null;
            await Parallel.ForAsync(0, _handlers.Length, async (int i, CancellationToken cancellationToken) =>
            {
                try
                {
                    await _handlers[i](eventArgs);
                }
                catch (Exception error)
                {
                    errors ??= [];
                    errors.Add(error);
                }
            });

            if (errors is null)
            {
                return;
            }

            throw errors.Count is 1 ? errors[0] : new AggregateException(errors);
        }
    }
}
