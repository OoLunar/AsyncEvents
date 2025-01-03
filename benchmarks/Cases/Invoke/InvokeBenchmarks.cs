using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace OoLunar.AsyncEvents.Benchmarks.Cases.Invoke
{
    public class InvokeBenchmarks
    {
        private readonly AsyncEventArgs _args = new();

        [Benchmark, ArgumentsSource("CreateAsyncEvents")]
        public async ValueTask InvokeAsync(IAsyncEvent<AsyncEventArgs> asyncEvent, string asyncEventName, int handlerCount) => await asyncEvent.InvokeAsync(_args);

        public static IEnumerable<object[]> CreateAsyncEvents() => BenchmarkHelper.CreateAsyncEvents();
    }
}
