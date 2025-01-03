using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using OoLunar.AsyncEvents.Benchmarks.Data;

namespace OoLunar.AsyncEvents.Benchmarks.Cases
{
    public class RemoveHandlerBenchmarks
    {
        [Benchmark, ArgumentsSource("CreateAsyncEvents")]
        public void RemovePostHandler(IAsyncEvent<AsyncEventArgs> asyncEvent, string asyncEventName, int handlerCount)
            => asyncEvent.RemovePostHandler(StaticEventHandlers.PostHandlerAsync);

        [Benchmark, ArgumentsSource("CreateAsyncEvents")]
        public void RemovePreHandler(IAsyncEvent<AsyncEventArgs> asyncEvent, string asyncEventName, int handlerCount)
            => asyncEvent.RemovePreHandler(StaticEventHandlers.PreHandlerAsync);

        public static IEnumerable<object[]> CreateAsyncEvents()
        {
            foreach (object[] args in BenchmarkHelper.CreateAsyncEvents())
            {
                IAsyncEvent<AsyncEventArgs> asyncEvent = (IAsyncEvent<AsyncEventArgs>)args[0];
                asyncEvent.RemovePostHandler(asyncEvent.PostHandlers[AsyncEventPriority.Normal][^1]);
                asyncEvent.AddPostHandler(StaticEventHandlers.PostHandlerAsync);

                asyncEvent.RemovePreHandler(asyncEvent.PreHandlers[AsyncEventPriority.Normal][^1]);
                asyncEvent.AddPreHandler(StaticEventHandlers.PreHandlerAsync);
                yield return args;
            }
        }
    }
}
