using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    public interface IAsyncEventPreHandler<TEventArgs> : IAsyncEventPreHandler where TEventArgs : AsyncEventArgs
    {
        Type IAsyncEventPreHandler.EventArgsType => typeof(TEventArgs);

        public ValueTask<bool> PreInvokeAsync(TEventArgs eventArgs, CancellationToken cancellationToken = default);

        ValueTask<bool> IAsyncEventPreHandler.PreInvokeAsync(AsyncEventArgs eventArgs, CancellationToken cancellationToken)
            => PreInvokeAsync((TEventArgs)eventArgs, cancellationToken);
    }
}
