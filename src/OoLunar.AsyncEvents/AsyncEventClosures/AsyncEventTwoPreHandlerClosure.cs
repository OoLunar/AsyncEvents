using System;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.AsyncEventClosures
{
    internal class AsyncEventTwoPreHandlerClosure<TAsyncEventArgs> where TAsyncEventArgs : AsyncEventArgs
    {
        private readonly AsyncEventPreHandler<TAsyncEventArgs> _handler1;
        private readonly AsyncEventPreHandler<TAsyncEventArgs> _handler2;

        public AsyncEventTwoPreHandlerClosure(AsyncEventPreHandler<TAsyncEventArgs> handler1, AsyncEventPreHandler<TAsyncEventArgs> handler2)
        {
            _handler1 = handler1;
            _handler2 = handler2;
        }

        public async ValueTask<bool> InvokeAsync(TAsyncEventArgs eventArgs)
        {
            bool result = true;
            Exception? error = null;
            try
            {
                result &= await _handler1(eventArgs);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            try
            {
                result &= await _handler2(eventArgs);
            }
            catch (Exception ex)
            {
                if (error is not null)
                {
                    throw new AggregateException(error, ex);
                }
            }

            return error is not null ? throw error : result;
        }
    }
}