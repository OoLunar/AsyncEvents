using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace OoLunar.AsyncEvents.Benchmarks
{
    public class CompareBenchmarks
    {
        private event AsyncEventPostHandler<AsyncEventArgs> _exampleEvent = default!;

        [Benchmark, ArgumentsSource(nameof(GeneratePostHandlers))]
        public async ValueTask InvokeNormalEventAsync(AsyncEvent<AsyncEventArgs> asyncEvent, AsyncEventArgs asyncEventArgs, int eventHandlerCount)
            => await _exampleEvent(asyncEventArgs);

        [Benchmark, ArgumentsSource(nameof(GeneratePostHandlers))]
        public async ValueTask InvokeAsyncEventAsync(AsyncEvent<AsyncEventArgs> asyncEvent, AsyncEventArgs asyncEventArgs, int eventHandlerCount)
            => await asyncEvent.InvokeAsync(asyncEventArgs);

        [Benchmark, ArgumentsSource(nameof(GenerateParallelPostHandlers))]
        public async ValueTask InvokeNormalEventParallelAsync(AsyncEvent<AsyncEventArgs> asyncEvent, AsyncEventArgs asyncEventArgs, int eventHandlerCount)
            => await _exampleEvent(asyncEventArgs);

        [Benchmark, ArgumentsSource(nameof(GenerateParallelPostHandlers))]
        public async ValueTask InvokeAsyncEventParallelAsync(AsyncEvent<AsyncEventArgs> asyncEvent, AsyncEventArgs asyncEventArgs, int eventHandlerCount)
            => await asyncEvent.InvokeAsync(asyncEventArgs);

        public IEnumerable<object[]> GeneratePostHandlers()
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
                    AsyncEventPostHandler<AsyncEventArgs> postHandler = preHandler.Compile().Method.CreateDelegate<AsyncEventPostHandler<AsyncEventArgs>>();
                    asyncEvent.AddPostHandler(postHandler);
                    _exampleEvent += postHandler;
                    j++;
                }

                asyncEvent.Prepare();
                yield return [asyncEvent, eventArgs, i];
            }
        }

        public IEnumerable<object[]> GenerateParallelPostHandlers()
        {
            // Generate a anonymous delegate through expressions, since adding
            // the same delegate multiple times will throw an exception
            Expression<Func<AsyncEventArgs, ValueTask>> preHandler = eventArgs => ValueTask.CompletedTask;

            AsyncEventArgs eventArgs = new();
            foreach (int i in Enumerable.Range(Environment.ProcessorCount, (Environment.ProcessorCount * Environment.ProcessorCount) + 1).Where(x => x % Environment.ProcessorCount == 0))
            {
                AsyncEvent<AsyncEventArgs> asyncEvent = new(true, 0);

                int j = 0;
                while (j < i)
                {
                    AsyncEventPostHandler<AsyncEventArgs> postHandler = preHandler.Compile().Method.CreateDelegate<AsyncEventPostHandler<AsyncEventArgs>>();
                    asyncEvent.AddPostHandler(postHandler);
                    _exampleEvent += postHandler;
                    j++;
                }

                asyncEvent.Prepare();
                yield return [asyncEvent, eventArgs, i];
            }
        }
    }
}
