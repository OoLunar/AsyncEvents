using System.Threading;
using System.Threading.Tasks;
using OoLunar.AsyncEvents.Tests.Data;

namespace OoLunar.AsyncEvents.Tests
{
    public sealed class EventHandlers
    {
        [AsyncEventHandler]
        public ValueTask<bool> PreHandler(TestAsyncEventArgs _, CancellationToken __) => ValueTask.FromResult(true);

        [AsyncEventHandler]
        public ValueTask PostHandler(TestAsyncEventArgs _, CancellationToken __) => ValueTask.CompletedTask;
    }
}
