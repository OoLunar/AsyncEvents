using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.AsyncEventClosures
{
    internal class AsyncEventMultiPreHandler<TAsyncEventArgs> where TAsyncEventArgs : AsyncEventArgs
    {
        private readonly AsyncEventPreHandler<TAsyncEventArgs>[] _handlers;

        public AsyncEventMultiPreHandler(AsyncEventPreHandler<TAsyncEventArgs>[] handlers)
        {
            ArgumentNullException.ThrowIfNull(handlers, nameof(handlers));
            ArgumentOutOfRangeException.ThrowIfZero(handlers.Length, nameof(handlers));
            _handlers = handlers;
        }

        public async ValueTask<bool> InvokeAsync(TAsyncEventArgs eventArgs)
        {
            bool result = true;
            List<Exception>? errors = null;
            for (int i = 0; i < _handlers.Length; i++)
            {
                try
                {
                    result &= await _handlers[i](eventArgs);
                }
                catch (Exception error)
                {
                    errors ??= [];
                    errors.Add(error);
                }
            }

            if (errors?.Count is null or 0)
            {
                return result;
            }
            else if (errors.Count is 1)
            {
                ExceptionDispatchInfo.Throw(errors[0]);
                return false; // This should never be reached
            }
            else
            {
                throw new AggregateException(errors);
            }
        }
    }
}
