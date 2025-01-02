using System.Threading.Tasks;

namespace OoLunar.AsyncEvents.Tests.Data
{
    public sealed class NonHandlers
    {
        public static ValueTask NonHandlerAsync(object obj) => ValueTask.CompletedTask;
    }
}
