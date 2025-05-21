using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.Benchmarks.Data
{
    public sealed class StaticEventHandlers
    {
        [AsyncEventHandlerPriority]
        public static ValueTask<bool> PreHandlerAsync(AsyncEventArgs _, CancellationToken __) => ValueTask.FromResult(true);

        [AsyncEventHandlerPriority]
        public static ValueTask PostHandlerAsync(AsyncEventArgs _, CancellationToken __) => ValueTask.CompletedTask;
    }
}
