using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    public interface IAsyncTaskEventPreHandler<TEventArgs> : IAsyncEventPreHandler<TEventArgs> where TEventArgs : AsyncEventArgs
    {
        public new Task<bool> PreInvokeAsync(TEventArgs eventArgs, CancellationToken cancellationToken = default);

        ValueTask<bool> IAsyncEventPreHandler<TEventArgs>.PreInvokeAsync(TEventArgs eventArgs, CancellationToken cancellationToken) => new(PreInvokeAsync(eventArgs, cancellationToken));
    }
}
