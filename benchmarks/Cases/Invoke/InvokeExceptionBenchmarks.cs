using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace OoLunar.AsyncEvents.Benchmarks.Cases.Invoke
{
    public class InvokeExceptionsBenchmarks
    {
        private event AsyncEventPostHandler<AsyncEventArgs> _postHandler = default!;
        private readonly AsyncEventArgs _args = new();

        [Benchmark, ArgumentsSource("CreateExceptionAsyncEvents")]
        public async ValueTask InvokeWithExceptionAsync(IAsyncEvent<AsyncEventArgs> asyncEvent, string asyncEventName, int handlerCount)
        {
            try
            {
                await asyncEvent.InvokeAsync(_args);
            }
            catch { }
        }

        [Benchmark, ArgumentsSource("SubscribeExceptionAsyncEvents")]
        public async ValueTask InvokeWithExceptionAsync(string asyncEventName, int handlerCount)
        {
            try
            {
                await _postHandler(_args);
            }
            catch { }
        }

        [Benchmark, ArgumentsSource("CreateFourExceptionAsyncEvents")]
        public async ValueTask InvokeWithFourExceptionsAsync(IAsyncEvent<AsyncEventArgs> asyncEvent, string asyncEventName, int handlerCount)
        {
            try
            {
                await asyncEvent.InvokeAsync(_args);
            }
            catch { }
        }

        [Benchmark, ArgumentsSource("SubscribeFourExceptionAsyncEvents")]
        public async ValueTask InvokeWithFourExceptionsAsync(string asyncEventName, int handlerCount)
        {
            try
            {
                await _postHandler(_args);
            }
            catch { }
        }

        public static IEnumerable<object[]> CreateExceptionAsyncEvents() => BenchmarkHelper.CreateAsyncEvents(true, true, 1);

        public static IEnumerable<object[]> CreateFourExceptionAsyncEvents() => BenchmarkHelper.CreateAsyncEvents(true, true, 4);

        public IEnumerable<object[]> SubscribeExceptionAsyncEvents()
        {
            // Generate a anonymous delegate through expressions, since adding
            // the same delegate multiple times will throw an exception
            Expression<Func<AsyncEventArgs, ValueTask<bool>>> preHandlerExpression = eventArgs => ValueTask.FromResult(true);
            Expression<Func<AsyncEventArgs, ValueTask>> postHandlerExpression = eventArgs => ValueTask.CompletedTask;

            IEnumerable<double> handlerCounts = Enumerable.Range(0, 10).Select(x => Math.Pow(2, x));
            foreach (double handlerCount in handlerCounts)
            {
                for (int i = 0; i < handlerCount; i++)
                {
                    AsyncEventPostHandler<AsyncEventArgs> postHandler = postHandlerExpression.Compile().Method.CreateDelegate<AsyncEventPostHandler<AsyncEventArgs>>();
                    _postHandler += postHandler;
                }

                _postHandler += args => throw new Exception();

                yield return [".NET Event", _postHandler.GetInvocationList().Length];
            }
        }

        public IEnumerable<object[]> SubscribeFourExceptionAsyncEvents()
        {
            // Generate a anonymous delegate through expressions, since adding
            // the same delegate multiple times will throw an exception
            Expression<Func<AsyncEventArgs, ValueTask<bool>>> preHandlerExpression = eventArgs => ValueTask.FromResult(true);
            Expression<Func<AsyncEventArgs, ValueTask>> postHandlerExpression = eventArgs => ValueTask.CompletedTask;

            IEnumerable<double> handlerCounts = Enumerable.Range(0, 10).Select(x => Math.Pow(2, x));
            foreach (double handlerCount in handlerCounts)
            {
                for (int i = 0; i < handlerCount - 4; i++)
                {
                    AsyncEventPostHandler<AsyncEventArgs> postHandler = postHandlerExpression.Compile().Method.CreateDelegate<AsyncEventPostHandler<AsyncEventArgs>>();
                    _postHandler += postHandler;
                }

                _postHandler += args => throw new Exception();
                _postHandler += args => throw new Exception();
                _postHandler += args => throw new Exception();
                _postHandler += args => throw new Exception();

                yield return [".NET Event", _postHandler.GetInvocationList().Length];
            }
        }
    }
}
