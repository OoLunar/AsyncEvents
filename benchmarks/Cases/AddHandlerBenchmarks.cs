using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using OoLunar.AsyncEvents.Benchmarks.Data;

namespace OoLunar.AsyncEvents.Benchmarks.Cases
{
    public class AddHandlerBenchmarks
    {
        private readonly InstanceEventHandlers _instanceEventHandlers = new();

        [Benchmark, ArgumentsSource("CreateAsyncEvents")]
        public void AddStaticHandlers(IAsyncEvent<AsyncEventArgs> asyncEvent, string asyncEventName)
            => asyncEvent.AddHandlers<StaticEventHandlers>();

        [Benchmark, ArgumentsSource("CreateAsyncEvents")]
        public void AddInstanceHandlers(IAsyncEvent<AsyncEventArgs> asyncEvent, string asyncEventName)
            => asyncEvent.AddHandlers<InstanceEventHandlers>(_instanceEventHandlers);

        [Benchmark, ArgumentsSource("CreateAsyncEvents")]
        public void AddPostHandler(IAsyncEvent<AsyncEventArgs> asyncEvent, string asyncEventName)
            => asyncEvent.AddPostHandler(StaticEventHandlers.PostHandlerAsync);

        [Benchmark, ArgumentsSource("CreateAsyncEvents")]
        public void AddPreHandler(IAsyncEvent<AsyncEventArgs> asyncEvent, string asyncEventName)
            => asyncEvent.AddPreHandler(StaticEventHandlers.PreHandlerAsync);

        public static IEnumerable<object[]> CreateAsyncEvents() => BenchmarkHelper.CreateAsyncEvents(false);
    }
}
