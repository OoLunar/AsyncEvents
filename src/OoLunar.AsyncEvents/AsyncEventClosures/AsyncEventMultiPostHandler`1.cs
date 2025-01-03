using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.AsyncEventClosures
{
    internal class AsyncEventMultiPostHandler<TAsyncEventArgs> where TAsyncEventArgs : AsyncEventArgs
    {
        private readonly AsyncEventPostHandler<TAsyncEventArgs>[] _handlers;

        public AsyncEventMultiPostHandler(AsyncEventPostHandler<TAsyncEventArgs>[] handlers) => _handlers = handlers;

        public async ValueTask InvokeAsync(TAsyncEventArgs eventArgs)
        {
            List<Exception>? errors = null;
            for (int i = 0; i < _handlers.Length; i++)
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
            }

            if (errors is null)
            {
                return;
            }

            throw errors.Count is 1 ? errors[0] : new AggregateException(errors);
        }
    }
}
