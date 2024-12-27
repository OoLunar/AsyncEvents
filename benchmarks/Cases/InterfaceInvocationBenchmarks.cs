using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace OoLunar.AsyncEvents.Benchmarks
{
    public class InterfaceInvocationBenchmarks
    {
        public sealed class MyAsyncEventArgs : AsyncEventArgs;

        [Benchmark, ArgumentsSource(nameof(GenerateData))]
        public ValueTask<bool> InterfaceAsync(IAsyncEvent asyncEvent, MyAsyncEventArgs asyncEventArgs, int eventHandlerCount)
            => asyncEvent.InvokeAsync(asyncEventArgs);

        [Benchmark, ArgumentsSource(nameof(GenerateData))]
        public async ValueTask GenericInterfaceAsync(IAsyncEvent<MyAsyncEventArgs> asyncEvent, MyAsyncEventArgs asyncEventArgs, int eventHandlerCount)
            => await asyncEvent.InvokeAsync(asyncEventArgs);

        [Benchmark, ArgumentsSource(nameof(GenerateData))]
        public async ValueTask InvokeAsync(AsyncEvent<MyAsyncEventArgs> asyncEvent, MyAsyncEventArgs asyncEventArgs, int eventHandlerCount)
            => await asyncEvent.InvokeAsync(asyncEventArgs);

        public IEnumerable<object[]> GenerateData()
        {
            // Generate a anonymous delegate through expressions, since adding
            // the same delegate multiple times will throw an exception
            Expression<Func<AsyncEventArgs, ValueTask<bool>>> preHandler = eventArgs => new ValueTask<bool>(true);
            Expression<Func<AsyncEventArgs, ValueTask>> postHandler = eventArgs => ValueTask.CompletedTask;

            MyAsyncEventArgs eventArgs = new();
            foreach (int i in new int[] { 0, 2 })
            {
                AsyncEvent<MyAsyncEventArgs> asyncEvent = new();

                int j = 0;
                while (j < i)
                {
                    asyncEvent.AddPreHandler(preHandler.Compile().Method.CreateDelegate<AsyncEventPreHandler<MyAsyncEventArgs>>());
                    asyncEvent.AddPostHandler(postHandler.Compile().Method.CreateDelegate<AsyncEventPostHandler<MyAsyncEventArgs>>());
                    j++;
                }

                asyncEvent.Prepare();
                yield return [asyncEvent, eventArgs, i];
            }
        }
    }
}
