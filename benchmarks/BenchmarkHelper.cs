using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using OoLunar.AsyncEvents.DebugAsyncEvents;
using OoLunar.AsyncEvents.ParallelAsyncEvents;

namespace OoLunar.AsyncEvents.Benchmarks
{
    public static class BenchmarkHelper
    {
        private const int HandlerCountsPow2 = 10;
        private static IEnumerable<IAsyncEvent<AsyncEventArgs>> _asyncEvents =>
        [
            new AsyncEvent<AsyncEventArgs>(),
            new ParallelAsyncEvent<AsyncEventArgs>(),
            new DebugAsyncEvent<AsyncEventArgs>(new AsyncEvent<AsyncEventArgs>(), NullLogger<DebugAsyncEvent<AsyncEventArgs>>.Instance)
        ];

        public static IEnumerable<object[]> CreateAsyncEvents(bool registerEventHandlers = true, bool prepareEventHandlers = false)
        {
            foreach (IAsyncEvent<AsyncEventArgs> asyncEvent in _asyncEvents)
            {
                if (!registerEventHandlers)
                {
                    yield return [asyncEvent, asyncEvent.GetType().Name];
                }
                else
                {
                    foreach (object[] asyncEventWithSubscribers in AddAsyncEventHandlers(asyncEvent, prepareEventHandlers))
                    {
                        yield return asyncEventWithSubscribers;
                    }
                }
            }
        }

        private static IEnumerable<object[]> AddAsyncEventHandlers(IAsyncEvent<AsyncEventArgs> asyncEvent, bool prepareEventHandlers)
        {
            // Generate a anonymous delegate through expressions, since adding
            // the same delegate multiple times will throw an exception
            Expression<Func<AsyncEventArgs, ValueTask<bool>>> preHandlerExpression = eventArgs => ValueTask.FromResult(true);
            Expression<Func<AsyncEventArgs, ValueTask>> postHandlerExpression = eventArgs => ValueTask.CompletedTask;

            IEnumerable<double> handlerCounts = Enumerable.Range(0, HandlerCountsPow2).Select(x => Math.Pow(2, x));
            foreach (double handlerCount in handlerCounts)
            {
                asyncEvent.ClearHandlers();
                for (int i = 0; i < handlerCount; i++)
                {
                    AsyncEventPreHandler<AsyncEventArgs> preHandler = preHandlerExpression.Compile().Method.CreateDelegate<AsyncEventPreHandler<AsyncEventArgs>>();
                    asyncEvent.AddPreHandler(preHandler);

                    AsyncEventPostHandler<AsyncEventArgs> postHandler = postHandlerExpression.Compile().Method.CreateDelegate<AsyncEventPostHandler<AsyncEventArgs>>();
                    asyncEvent.AddPostHandler(postHandler);

                    if (prepareEventHandlers)
                    {
                        asyncEvent.Prepare();
                    }
                }

                yield return [asyncEvent, asyncEvent.GetType().Name, (int)handlerCount];
            }
        }
    }
}
