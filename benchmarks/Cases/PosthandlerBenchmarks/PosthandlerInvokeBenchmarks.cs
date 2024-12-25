using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace OoLunar.AsyncEvents.Benchmarks.Posthandlers
{
    public class PosthandlerInvokeBenchmarks
    {
        [Benchmark, ArgumentsSource(nameof(InvokePostHandlersData))]
        public async ValueTask InvokePostHandlersAsync(AsyncEvent<AsyncEventArgs> asyncEvent, AsyncEventArgs asyncEventArgs, int eventHandlerCount)
            => await asyncEvent.InvokePostHandlersAsync(asyncEventArgs);

        public static IEnumerable<object[]> InvokePostHandlersData()
        {
            // Generate a anonymous delegate through expressions, since adding
            // the same delegate multiple times will throw an exception
            Expression<Func<AsyncEventArgs, ValueTask>> preHandler = eventArgs => ValueTask.CompletedTask;

            AsyncEventArgs eventArgs = new();
            foreach (int i in Enumerable.Range(0, Environment.ProcessorCount + 1).Where(x => x % 4 == 0).Append(1).Append(2).Append(3).Append(5).OrderByDescending(x => x))
            {
                AsyncEvent<AsyncEventArgs> asyncEvent = new();

                int j = 0;
                while (j < i)
                {
                    asyncEvent.AddPostHandler(preHandler.Compile().Method.CreateDelegate<AsyncEventPostHandler<AsyncEventArgs>>());
                    j++;
                }

                asyncEvent.Prepare();
                yield return [asyncEvent, eventArgs, i];
            }
        }
    }
}
