using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.Tests.Data
{
    public class StaticEventHandlers
    {
        [AsyncEventHandlerPriority]
        public static ValueTask<bool> PreHandler(TestAsyncEventArgs _, CancellationToken __) => ValueTask.FromResult(true);

        [AsyncEventHandlerPriority]
        public static ValueTask PostHandler(TestAsyncEventArgs _, CancellationToken __) => ValueTask.CompletedTask;
    }
}
