using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.Benchmarks.Data
{
    public sealed class InstanceEventHandlers : IAsyncEventPreHandler<AsyncEventArgs>, IAsyncEventPostHandler<AsyncEventArgs>
    {
        public ValueTask<bool> PreInvokeAsync(AsyncEventArgs eventArgs, CancellationToken cancellationToken = default) => ValueTask.FromResult(true);
        public ValueTask InvokeAsync(AsyncEventArgs eventArgs, CancellationToken cancellationToken) => ValueTask.CompletedTask;
    }
}
