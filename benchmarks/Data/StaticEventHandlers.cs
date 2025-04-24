using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.Benchmarks.Data
{
    public sealed class StaticEventHandlers
    {
        [AsyncEventHandler]
        public static ValueTask<bool> PreHandlerAsync(AsyncEventArgs _, CancellationToken __) => ValueTask.FromResult(true);

        [AsyncEventHandler]
        public static ValueTask PostHandlerAsync(AsyncEventArgs _, CancellationToken __) => ValueTask.CompletedTask;
    }
}
