using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    public interface IAsyncTaskEventPostHandler<TEventArgs> : IAsyncEventPostHandler<TEventArgs> where TEventArgs : AsyncEventArgs
    {
        public new Task InvokeAsync(TEventArgs eventArgs, CancellationToken cancellationToken = default);

        ValueTask IAsyncEventPostHandler<TEventArgs>.InvokeAsync(TEventArgs eventArgs, CancellationToken cancellationToken) => new(InvokeAsync(eventArgs, cancellationToken));
    }
}
