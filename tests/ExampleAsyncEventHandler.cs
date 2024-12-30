using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.Tests
{
    public sealed class ExampleAsyncEventHandler
    {
        [AsyncEventHandler]
        public static async ValueTask<bool> ExamplePreHandlerAsync(AsyncEventArgs eventArgs)
        {
            await Task.Delay(1000);
            return true;
        }

        [AsyncEventHandler]
        public static async ValueTask ExamplePostHandlerAsync(AsyncEventArgs eventArgs) => await Task.Delay(1000);
    }
}
