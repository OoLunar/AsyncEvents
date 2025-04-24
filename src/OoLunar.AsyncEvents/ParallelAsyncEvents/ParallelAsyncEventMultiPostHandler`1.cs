using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.ParallelAsyncEvents
{
    internal class ParallelAsyncEventMultiPostHandler<TAsyncEventArgs> where TAsyncEventArgs : AsyncEventArgs
    {
        private readonly AsyncEventPostHandler<TAsyncEventArgs>[] _handlers;

        public ParallelAsyncEventMultiPostHandler(AsyncEventPostHandler<TAsyncEventArgs>[] handlers)
        {
            ArgumentNullException.ThrowIfNull(handlers, nameof(handlers));
            ArgumentOutOfRangeException.ThrowIfZero(handlers.Length, nameof(handlers));
            _handlers = handlers;
        }

        public async ValueTask InvokeAsync(TAsyncEventArgs eventArgs, CancellationToken cancellationToken = default)
        {
            List<Exception>? errors = null;
            await Parallel.ForAsync(0, _handlers.Length, cancellationToken, async (int i, CancellationToken cancellationToken) =>
            {
                try
                {
                    await _handlers[i](eventArgs, cancellationToken);
                }
                catch (Exception error)
                {
                    errors ??= [];
                    errors.Add(error);
                }
            });

            if (errors?.Count is null or 0)
            {
                return;
            }
            else if (errors.Count is 1)
            {
                ExceptionDispatchInfo.Throw(errors[0]);
            }
            else
            {
                throw new AggregateException(errors);
            }
        }
    }
}
