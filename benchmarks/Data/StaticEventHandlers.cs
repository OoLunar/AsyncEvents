using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.Benchmarks.Data
{
    public sealed class StaticEventHandlers
    {
        [AsyncEventHandler]
        public static ValueTask<bool> PreHandlerAsync(AsyncEventArgs _) => ValueTask.FromResult(true);

        [AsyncEventHandler]
        public static ValueTask PostHandlerAsync(AsyncEventArgs _) => ValueTask.CompletedTask;
    }
}
