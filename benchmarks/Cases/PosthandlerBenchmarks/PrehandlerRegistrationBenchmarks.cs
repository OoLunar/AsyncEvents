using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace OoLunar.AsyncEvents.Benchmarks.Posthandlers
{
    public class PosthandlerRegistrationBenchmarks
    {
        [Benchmark, ArgumentsSource(nameof(CreateEmptyAsyncEvent))]
        public void AddPostHandler(AsyncEvent<AsyncEventArgs> asyncEvent) => asyncEvent.AddPostHandler(EmptyPostHandler);

        [Benchmark, ArgumentsSource(nameof(CreateEmptyAsyncEvent))]
        public void RemovePostHandler(AsyncEvent<AsyncEventArgs> asyncEvent) => asyncEvent.RemovePostHandler(EmptyPostHandler);

        public static IEnumerable<AsyncEvent<AsyncEventArgs>> CreateEmptyAsyncEvent()
        {
            yield return new();
        }

        private static ValueTask EmptyPostHandler(AsyncEventArgs _) => default;
    }
}
