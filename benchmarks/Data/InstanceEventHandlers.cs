using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.Benchmarks.Data
{
    public sealed class InstanceEventHandlers
    {
        [AsyncEventHandler]
        public ValueTask<bool> PreHandlerAsync(AsyncEventArgs _) => ValueTask.FromResult(true);

        [AsyncEventHandler]
        public ValueTask PostHandlerAsync(AsyncEventArgs _) => ValueTask.CompletedTask;
    }
}
