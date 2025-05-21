using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    public interface IAsyncEventPostHandler<TEventArgs> : IAsyncEventPostHandler where TEventArgs : AsyncEventArgs
    {
        Type IAsyncEventPostHandler.EventArgsType => typeof(TEventArgs);

        public ValueTask InvokeAsync(TEventArgs eventArgs, CancellationToken cancellationToken = default);

        ValueTask IAsyncEventPostHandler.InvokeAsync(AsyncEventArgs eventArgs, CancellationToken cancellationToken)
            => InvokeAsync((TEventArgs)eventArgs, cancellationToken);
    }
}
