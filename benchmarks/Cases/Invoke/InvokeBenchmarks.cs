using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace OoLunar.AsyncEvents.Benchmarks.Cases.Invoke
{
    public class InvokeBenchmarks
    {
        private event AsyncEventPostHandler<AsyncEventArgs> _asyncEvent = default!;
        private static readonly AsyncEventArgs _args = new();

        [Benchmark, ArgumentsSource("CreateAsyncEvents")]
        public async ValueTask InvokeAsync(IAsyncEvent<AsyncEventArgs> asyncEvent, string asyncEventName, int handlerCount) => await asyncEvent.InvokePostHandlersAsync(_args);

        [Benchmark, ArgumentsSource("SubscribeAsyncEvents")]
        public async ValueTask InvokeAsync(AsyncEventPostHandler<AsyncEventArgs> asyncEvent, string asyncEventName, int handlerCount) => await asyncEvent(_args);

        public static IEnumerable<object[]> CreateAsyncEvents() => BenchmarkHelper.CreateAsyncEvents();

        public IEnumerable<object[]> SubscribeAsyncEvents()
        {
            foreach (int handlerCount in BenchmarkHelper.HandlerCounts)
            {
                // Clear the previous event handlers
                _asyncEvent = default!;

                // Add the new event handlers
                foreach (AsyncEventPostHandler<AsyncEventArgs> postHandler in BenchmarkHelper.CreatePostHandlers(handlerCount))
                {
                    _asyncEvent += postHandler;
                }

                AsyncEventPostHandler<AsyncEventArgs> asyncEvent = _asyncEvent;
                yield return [asyncEvent, ".NET Event", _asyncEvent.GetInvocationList().Length];
            }
        }
    }
}
