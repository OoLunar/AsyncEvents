using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace OoLunar.AsyncEvents.Benchmarks.Cases
{
    public class PrepareBenchmarks
    {
        [Benchmark, ArgumentsSource(nameof(PrepareArguments))]
        public void Prepare(IAsyncEvent<AsyncEventArgs> asyncEvent, string asyncEventName, int handlerCount) => asyncEvent.Prepare();

        [Benchmark, ArgumentsSource(nameof(PrepareArguments))]
        public async ValueTask PrepareAsync(IAsyncEvent<AsyncEventArgs> asyncEvent, string asyncEventName, int handlerCount) => await asyncEvent.PrepareAsync();

        public static IEnumerable<object[]> PrepareArguments() => BenchmarkHelper.CreateAsyncEvents();
    }
}
