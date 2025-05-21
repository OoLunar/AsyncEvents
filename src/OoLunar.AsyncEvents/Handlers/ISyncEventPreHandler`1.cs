using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    public interface ISyncEventPreHandler<TEventArgs> : IAsyncEventPreHandler<TEventArgs> where TEventArgs : AsyncEventArgs
    {
        public bool PreInvoke(TEventArgs eventArgs, CancellationToken cancellationToken = default);

        ValueTask<bool> IAsyncEventPreHandler<TEventArgs>.PreInvokeAsync(TEventArgs eventArgs, CancellationToken cancellationToken)
            => ValueTask.FromResult(PreInvoke(eventArgs, cancellationToken));
    }
}
