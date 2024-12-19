using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace OoLunar.AsyncEvents.Benchmarks.Prehandlers
{
    public class PrehandlerRegistrationBenchmarks
    {
        [Benchmark, ArgumentsSource(nameof(CreateEmptyAsyncEvent))]
        public void AddPreHandler(AsyncEvent<AsyncEventArgs> asyncEvent) => asyncEvent.AddPreHandler(EmptyPreHandler);

        [Benchmark, ArgumentsSource(nameof(CreateEmptyAsyncEvent))]
        public void RemovePreHandler(AsyncEvent<AsyncEventArgs> asyncEvent) => asyncEvent.RemovePreHandler(EmptyPreHandler);

        public static IEnumerable<AsyncEvent<AsyncEventArgs>> CreateEmptyAsyncEvent()
        {
            yield return new();
        }

        private static ValueTask<bool> EmptyPreHandler(AsyncEventArgs _) => new(true);
    }
}
