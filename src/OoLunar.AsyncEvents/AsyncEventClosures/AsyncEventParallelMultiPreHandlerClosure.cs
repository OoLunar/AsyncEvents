using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.AsyncEventClosures
{
    internal class AsyncEventParallelMultiPreHandlerClosure<TAsyncEventArgs> where TAsyncEventArgs : AsyncEventArgs
    {
        private readonly AsyncEventPreHandler<TAsyncEventArgs>[] _handlers;

        public AsyncEventParallelMultiPreHandlerClosure(AsyncEventPreHandler<TAsyncEventArgs>[] handlers) => _handlers = handlers;

        public async ValueTask<bool> InvokeAsync(TAsyncEventArgs eventArgs)
        {
            bool result = true;
            List<Exception> errors = [];
            await Parallel.ForAsync(0, _handlers.Length, async (int i, CancellationToken cancellationToken) =>
            {
                try
                {
                    result &= await _handlers[i](eventArgs);
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

            return result;
        }
    }
}
