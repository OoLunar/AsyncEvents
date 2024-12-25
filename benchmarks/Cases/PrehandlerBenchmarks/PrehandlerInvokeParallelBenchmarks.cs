using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace OoLunar.AsyncEvents.Benchmarks.Prehandlers
{
    public class PrehandlerInvokeParallelBenchmarks
    {
        [Benchmark, ArgumentsSource(nameof(InvokePreHandlersData))]
        public async ValueTask InvokePreHandlersParallelAsync(AsyncEvent<AsyncEventArgs> asyncEvent, AsyncEventArgs asyncEventArgs, int eventHandlerCount)
            => await asyncEvent.InvokePreHandlersAsync(asyncEventArgs);

        public static IEnumerable<object[]> InvokePreHandlersData()
        {
            // Generate a anonymous delegate through expressions, since adding
            // the same delegate multiple times will throw an exception
            Expression<Func<AsyncEventArgs, ValueTask<bool>>> preHandler = eventArgs => new ValueTask<bool>(true);

            AsyncEventArgs eventArgs = new();
            foreach (int i in Enumerable.Range(Environment.ProcessorCount, (Environment.ProcessorCount * Environment.ProcessorCount) + 1).Where(x => x % Environment.ProcessorCount == 0))
            {
                AsyncEvent<AsyncEventArgs> asyncEvent = new(true, 0);

                int j = 0;
                while (j < i)
                {
                    asyncEvent.AddPreHandler(preHandler.Compile().Method.CreateDelegate<AsyncEventPreHandler<AsyncEventArgs>>());
                    j++;
                }

                asyncEvent.Prepare();
                yield return [asyncEvent, eventArgs, i];
            }
        }
    }
}
