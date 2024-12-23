using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.AsyncEventClosures
{
    internal class AsyncEventMultiPostHandlerClosure<TAsyncEventArgs> where TAsyncEventArgs : AsyncEventArgs
    {
        private readonly AsyncEventPostHandler<TAsyncEventArgs>[] _handlers;

        public AsyncEventMultiPostHandlerClosure(AsyncEventPostHandler<TAsyncEventArgs>[] handlers) => _handlers = handlers;

        public async ValueTask InvokeAsync(TAsyncEventArgs eventArgs)
        {
            List<Exception> errors = [];
            for (int i = 0; i < _handlers.Length; i++)
            {
                try
                {
                    await _handlers[i](eventArgs);
                }
                catch (Exception error)
                {
                    errors.Add(error);
                }
            }

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
