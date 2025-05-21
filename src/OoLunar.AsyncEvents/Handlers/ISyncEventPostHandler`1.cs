using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    public interface ISyncEventPostHandler<TEventArgs> : IAsyncEventPostHandler<TEventArgs> where TEventArgs : AsyncEventArgs
    {
        public void Invoke(TEventArgs eventArgs, CancellationToken cancellationToken = default);

        ValueTask IAsyncEventPostHandler<TEventArgs>.InvokeAsync(TEventArgs eventArgs, CancellationToken cancellationToken)
        {
            Invoke(eventArgs, cancellationToken);
            return ValueTask.CompletedTask;
        }
    }
}
