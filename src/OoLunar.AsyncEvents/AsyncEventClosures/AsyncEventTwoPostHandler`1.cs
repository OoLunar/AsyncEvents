using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.AsyncEventClosures
{
    internal class AsyncEventTwoPostHandler<TAsyncEventArgs> where TAsyncEventArgs : AsyncEventArgs
    {
        private readonly AsyncEventPostHandler<TAsyncEventArgs> _handler1;
        private readonly AsyncEventPostHandler<TAsyncEventArgs> _handler2;

        public AsyncEventTwoPostHandler(AsyncEventPostHandler<TAsyncEventArgs> handler1, AsyncEventPostHandler<TAsyncEventArgs> handler2)
        {
            _handler1 = handler1;
            _handler2 = handler2;
        }

        public async ValueTask InvokeAsync(TAsyncEventArgs eventArgs, CancellationToken cancellationToken = default)
        {
            Exception? error = null;
            try
            {
                await _handler1(eventArgs, cancellationToken);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            try
            {
                await _handler2(eventArgs, cancellationToken);
            }
            catch (Exception ex)
            {
                error = error is null ? ex : throw new AggregateException(error, ex);
            }

            if (error is not null)
            {
                ExceptionDispatchInfo.Throw(error);
            }
        }
    }
}
