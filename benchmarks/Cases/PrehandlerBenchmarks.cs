using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace OoLunar.AsyncEvents.Benchmarks
{
    public class PrehandlerBenchmarks
    {
        private readonly AsyncEvent<AsyncEventArgs> _asyncEvent = new();

        [Benchmark]
        public void AddPreHandler() => _asyncEvent.AddPreHandler(EmptyPreHandler);

        [Benchmark]
        public void RemovePreHandler() => _asyncEvent.RemovePreHandler(EmptyPreHandler);

        [Benchmark, ArgumentsSource(nameof(InvokePreHandlersData))]
        public async ValueTask InvokePreHandlersAsync(AsyncEvent<AsyncEventArgs> asyncEvent, AsyncEventArgs asyncEventArgs, int eventHandlerCount) => await asyncEvent.InvokePreHandlersAsync(asyncEventArgs);

        [Benchmark, ArgumentsSource(nameof(PreparePreHandlersData))]
        public void Prepare(AsyncEvent<AsyncEventArgs> asyncEvent) => asyncEvent.Prepare();

        [IterationSetup(Target = nameof(AddPreHandler))]
        public void SetupAddPreHandler() => _asyncEvent.RemovePreHandler(EmptyPreHandler);

        [IterationSetup(Target = nameof(RemovePreHandler))]
        public void SetupRemovePreHandler() => _asyncEvent.AddPreHandler(EmptyPreHandler);

        public static IEnumerable<object[]> InvokePreHandlersData()
        {
            // Generate a anonymous delegate through expressions, since adding
            // the same delegate multiple times will throw an exception
            Expression<Func<AsyncEventArgs, ValueTask<bool>>> preHandler = eventArgs => new ValueTask<bool>(true);

            List<AsyncEvent<AsyncEventArgs>> asyncEvents = [];
            foreach (int i in Enumerable.Range(0, Environment.ProcessorCount + 1).Where(x => x % 4 == 0).Append(1).Append(2).Append(5).Append(3).OrderByDescending(x => x))
            {
                AsyncEvent<AsyncEventArgs> asyncEvent = new();
                asyncEvents.Add(asyncEvent);
                int j = 0;
                while (j < i)
                {
                    asyncEvent.AddPreHandler(preHandler.Compile().Method.CreateDelegate<AsyncEventPreHandler<AsyncEventArgs>>());
                    j++;
                }

                asyncEvent.Prepare();
                yield return [asyncEvent, new AsyncEventArgs(), i];
            }
        }

        public static IEnumerable<object[]> PreparePreHandlersData()
        {
            foreach (object[] data in InvokePreHandlersData())
            {
                AsyncEvent<AsyncEventArgs> asyncEvent = (AsyncEvent<AsyncEventArgs>)data[0];
                asyncEvent.Prepare();
                yield return [asyncEvent, data[2]];
            }
        }

        private static ValueTask<bool> EmptyPreHandler(AsyncEventArgs _) => new(true);
    }
}
