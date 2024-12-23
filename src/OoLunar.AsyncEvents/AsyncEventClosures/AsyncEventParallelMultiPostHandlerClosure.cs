using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.AsyncEventClosures
{
    internal class AsyncEventParallelMultiPostHandlerClosure<TAsyncEventArgs> where TAsyncEventArgs : AsyncEventArgs
    {
        private readonly AsyncEventHandler<TAsyncEventArgs>[] _handlers;

        public AsyncEventParallelMultiPostHandlerClosure(AsyncEventHandler<TAsyncEventArgs>[] handlers) => _handlers = handlers;

        public async ValueTask InvokeAsync(TAsyncEventArgs eventArgs)
        {
            List<Exception> errors = [];
            await Parallel.ForAsync(0, _handlers.Length, async (int i, CancellationToken cancellationToken) =>
            {
                try
                {
                    await _handlers[i](eventArgs);
                }
                catch (Exception error)
                {
                    errors.Add(error);
                }
            });

            if (errors.Count == 1)
            {
                throw errors[0];
            }
            else if (errors.Count > 1)
            {
                throw new AggregateException(errors);
            }
        }
    }
}
