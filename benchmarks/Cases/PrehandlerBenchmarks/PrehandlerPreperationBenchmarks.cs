using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace OoLunar.AsyncEvents.Benchmarks.Prehandlers
{
    public class PrehandlerPreperationBenchmarks
    {
        [Benchmark, ArgumentsSource(nameof(PreparePreHandlersData))]
        public void Prepare(AsyncEvent<AsyncEventArgs> asyncEvent, int eventHandlerCount) => asyncEvent.Prepare();

        public static IEnumerable<object[]> PreparePreHandlersData()
        {
            // Generate a anonymous delegate through expressions, since adding
            // the same delegate multiple times will throw an exception
            Expression<Func<AsyncEventArgs, ValueTask<bool>>> preHandler = eventArgs => new ValueTask<bool>(true);
            foreach (int i in Enumerable.Range(0, Environment.ProcessorCount + 1).Where(x => x % 4 == 0).Append(1).Append(2).Append(3).Append(5).Distinct().OrderByDescending(x => x))
            {
                AsyncEvent<AsyncEventArgs> asyncEvent = new();

                int j = 0;
                while (j < i)
                {
                    asyncEvent.AddPreHandler(preHandler.Compile().Method.CreateDelegate<AsyncEventPreHandler<AsyncEventArgs>>());
                    j++;
                }

                yield return [asyncEvent, i];
            }
        }
    }
}
