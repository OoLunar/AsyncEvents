using System;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.AsyncEventClosures
{
    internal class AsyncEventTwoPostHandlerClosure<TAsyncEventArgs> where TAsyncEventArgs : AsyncEventArgs
    {
        private readonly AsyncEventHandler<TAsyncEventArgs> _handler1;
        private readonly AsyncEventHandler<TAsyncEventArgs> _handler2;

        public AsyncEventTwoPostHandlerClosure(AsyncEventHandler<TAsyncEventArgs> handler1, AsyncEventHandler<TAsyncEventArgs> handler2)
        {
            _handler1 = handler1;
            _handler2 = handler2;
        }

        public async ValueTask InvokeAsync(TAsyncEventArgs eventArgs)
        {
            Exception? error = null;
            try
            {
                await _handler1(eventArgs);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            try
            {
                await _handler2(eventArgs);
            }
            catch (Exception ex)
            {
                if (error is not null)
                {
                    throw new AggregateException(error, ex);
                }
            }

            if (error is not null)
            {
                throw error;
            }
        }
    }
}
