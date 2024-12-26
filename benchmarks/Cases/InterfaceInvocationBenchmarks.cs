using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace OoLunar.AsyncEvents.Benchmarks
{
    public class InterfaceInvocationBenchmarks
    {
        public sealed class MyAsyncEventArgs : AsyncEventArgs;

        [Benchmark, ArgumentsSource(nameof(GenerateData))]
        public async ValueTask InvokePreHandlersAsync(IAsyncEvent<MyAsyncEventArgs> asyncEvent, MyAsyncEventArgs asyncEventArgs, int eventHandlerCount)
            => await asyncEvent.InvokePreHandlersAsync(asyncEventArgs);

        [Benchmark, ArgumentsSource(nameof(GenerateData))]
        public async ValueTask InvokePostHandlersAsync(IAsyncEvent<MyAsyncEventArgs> asyncEvent, MyAsyncEventArgs asyncEventArgs, int eventHandlerCount)
            => await asyncEvent.InvokePostHandlersAsync(asyncEventArgs);

        [Benchmark, ArgumentsSource(nameof(GenerateData))]
        public async ValueTask InvokeAsync(IAsyncEvent<MyAsyncEventArgs> asyncEvent, MyAsyncEventArgs asyncEventArgs, int eventHandlerCount)
            => await asyncEvent.InvokeAsync(asyncEventArgs);

        public IEnumerable<object[]> GenerateData()
        {
            // Generate a anonymous delegate through expressions, since adding
            // the same delegate multiple times will throw an exception
            Expression<Func<AsyncEventArgs, ValueTask<bool>>> preHandler = eventArgs => new ValueTask<bool>(true);
            Expression<Func<AsyncEventArgs, ValueTask>> postHandler = eventArgs => ValueTask.CompletedTask;

            MyAsyncEventArgs eventArgs = new();
            foreach (int i in Enumerable.Range(0, Environment.ProcessorCount + 1).Where(x => x % 4 == 0).Append(1).Append(2).Append(3).Append(5).OrderByDescending(x => x))
            {
                AsyncEvent<AsyncEventArgs> asyncEvent = new();

                int j = 0;
                while (j < i)
                {
                    asyncEvent.AddPreHandler(preHandler.Compile().Method.CreateDelegate<AsyncEventPreHandler<AsyncEventArgs>>());
                    asyncEvent.AddPostHandler(postHandler.Compile().Method.CreateDelegate<AsyncEventPostHandler<AsyncEventArgs>>());
                    j++;
                }

                asyncEvent.Prepare();
                yield return [asyncEvent, eventArgs, i];
            }
        }
    }
}
