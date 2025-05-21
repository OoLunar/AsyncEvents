using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    internal class AsyncEventTwoPreHandler<TAsyncEventArgs> where TAsyncEventArgs : AsyncEventArgs
    {
        private readonly AsyncEventPreHandler<TAsyncEventArgs> _handler1;
        private readonly AsyncEventPreHandler<TAsyncEventArgs> _handler2;

        public AsyncEventTwoPreHandler(AsyncEventPreHandler<TAsyncEventArgs> handler1, AsyncEventPreHandler<TAsyncEventArgs> handler2)
        {
            _handler1 = handler1;
            _handler2 = handler2;
        }

        public async ValueTask<bool> InvokeAsync(TAsyncEventArgs eventArgs, CancellationToken cancellationToken = default)
        {
            bool result = true;
            Exception? error = null;
            try
            {
                result &= await _handler1(eventArgs, cancellationToken);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            try
            {
                result &= await _handler2(eventArgs, cancellationToken);
            }
            catch (Exception ex)
            {
                error = error is null ? ex : throw new AggregateException(error, ex);
            }

            if (error is not null)
            {
                ExceptionDispatchInfo.Throw(error);
            }

            return result;
        }
    }
}
