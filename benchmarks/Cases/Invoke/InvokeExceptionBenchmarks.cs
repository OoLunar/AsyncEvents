using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace OoLunar.AsyncEvents.Benchmarks.Cases.Invoke
{
    public class InvokeExceptionsBenchmarks
    {
        private static readonly Exception _exception = new();
        private static readonly AsyncEventArgs _args = new();

        private event AsyncEventPostHandler<AsyncEventArgs> _asyncEvent = default!;

        [Benchmark, ArgumentsSource("CreateExceptionAsyncEvents")]
        public async ValueTask InvokeWithExceptionAsync(IAsyncEvent<AsyncEventArgs> asyncEvent, string asyncEventName, int handlerCount)
        {
            try
            {
                await asyncEvent.InvokePostHandlersAsync(_args);
            }
            catch { }
        }

        [Benchmark, ArgumentsSource("SubscribeExceptionAsyncEvents")]
        public async ValueTask InvokeWithExceptionAsync(AsyncEventPostHandler<AsyncEventArgs> asyncEvent, string asyncEventName, int handlerCount)
        {
            try
            {
                await asyncEvent(_args);
            }
            catch { }
        }

        [Benchmark, ArgumentsSource("CreateFourExceptionAsyncEvents")]
        public async ValueTask InvokeWithFourExceptionsAsync(IAsyncEvent<AsyncEventArgs> asyncEvent, string asyncEventName, int handlerCount)
        {
            try
            {
                await asyncEvent.InvokePostHandlersAsync(_args);
            }
            catch { }
        }

        [Benchmark, ArgumentsSource("SubscribeFourExceptionAsyncEvents")]
        public async ValueTask InvokeWithFourExceptionsAsync(AsyncEventPostHandler<AsyncEventArgs> asyncEvent, string asyncEventName, int handlerCount)
        {
            try
            {
                await asyncEvent(_args);
            }
            catch { }
        }

        public static IEnumerable<object[]> CreateExceptionAsyncEvents() => BenchmarkHelper.CreateAsyncEvents(true, true, 1);

        public static IEnumerable<object[]> CreateFourExceptionAsyncEvents() => BenchmarkHelper.CreateAsyncEvents(true, true, 4);

        public IEnumerable<object[]> SubscribeExceptionAsyncEvents()
        {
            foreach (int handlerCount in BenchmarkHelper.HandlerCounts)
            {
                // Clear the previous event handlers
                _asyncEvent = default!;

                // Add the new event handlers
                foreach (AsyncEventPostHandler<AsyncEventArgs> postHandler in BenchmarkHelper.CreatePostHandlers(handlerCount - 1))
                {
                    _asyncEvent += postHandler;
                }

                _asyncEvent += (args, cancellation) => throw _exception;

                AsyncEventPostHandler<AsyncEventArgs> asyncEvent = _asyncEvent;
                yield return [asyncEvent, ".NET Event", _asyncEvent.GetInvocationList().Length];
            }
        }

        public IEnumerable<object[]> SubscribeFourExceptionAsyncEvents()
        {
            foreach (int handlerCount in BenchmarkHelper.HandlerCounts)
            {
                // Clear the previous event handlers
                _asyncEvent = default!;

                // Add the new event handlers
                foreach (AsyncEventPostHandler<AsyncEventArgs> postHandler in BenchmarkHelper.CreatePostHandlers(handlerCount - 4))
                {
                    _asyncEvent += postHandler;
                }

                // Fill the remaining event handlers with exceptions
                for (int i = 0; i < Math.Min(4, handlerCount); i++)
                {
                    _asyncEvent += (args, cancellation) => throw _exception;
                }

                AsyncEventPostHandler<AsyncEventArgs> asyncEvent = _asyncEvent;
                yield return [asyncEvent, ".NET Event", _asyncEvent.GetInvocationList().Length];
            }
        }
    }
}
