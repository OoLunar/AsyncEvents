using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace OoLunar.AsyncEvents.Benchmarks.Cases
{
    public class ClearBenchmarks
    {
        [Benchmark, ArgumentsSource(nameof(ClearArguments))]
        public void Clear(IAsyncEvent<AsyncEventArgs> asyncEvent, string asyncEventName, int handlerCount) => asyncEvent.ClearHandlers();

        [Benchmark, ArgumentsSource(nameof(ClearArguments))]
        public void ClearPostHandlers(IAsyncEvent<AsyncEventArgs> asyncEvent, string asyncEventName, int handlerCount) => asyncEvent.ClearPostHandlers();

        [Benchmark, ArgumentsSource(nameof(ClearArguments))]
        public void ClearPreHandlers(IAsyncEvent<AsyncEventArgs> asyncEvent, string asyncEventName, int handlerCount) => asyncEvent.ClearPreHandlers();

        public static IEnumerable<object[]> ClearArguments() => BenchmarkHelper.CreateAsyncEvents();
    }
}
