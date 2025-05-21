using System.Threading;
using System.Threading.Tasks;
using OoLunar.AsyncEvents.Tests.Data;

namespace OoLunar.AsyncEvents.Tests
{
    public sealed class EventHandlers : IAsyncEventPostHandler<TestAsyncEventArgs>, IAsyncEventPreHandler<TestAsyncEventArgs>
    {
        public ValueTask<bool> PreInvokeAsync(TestAsyncEventArgs _, CancellationToken __) => ValueTask.FromResult(true);

        public ValueTask InvokeAsync(TestAsyncEventArgs _, CancellationToken __) => ValueTask.CompletedTask;
    }
}
