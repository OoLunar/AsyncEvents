using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using OoLunar.AsyncEvents.DebugAsyncEvents;
using OoLunar.AsyncEvents.ParallelAsyncEvents;

namespace OoLunar.AsyncEvents.Benchmarks
{
    public static class BenchmarkHelper
    {
        private const int HandlerCountsPow2 = 10;

        public static IEnumerable<int> HandlerCounts => Enumerable.Range(0, HandlerCountsPow2).Select(x => (int)Math.Pow(2, x));
        public static IEnumerable<IAsyncEvent<AsyncEventArgs>> AsyncEvents =>
        [
            new AsyncEvent<AsyncEventArgs>(),
            new ParallelAsyncEvent<AsyncEventArgs>(),
            new DebugAsyncEvent<AsyncEventArgs>(new AsyncEvent<AsyncEventArgs>(), NullLogger<DebugAsyncEvent<AsyncEventArgs>>.Instance)
        ];

        public static IEnumerable<object[]> CreateAsyncEvents(bool registerEventHandlers = true, bool prepareEventHandlers = false, int exceptionHandlerCount = 0)
        {
            foreach (IAsyncEvent<AsyncEventArgs> asyncEvent in AsyncEvents)
            {
                if (!registerEventHandlers)
                {
                    yield return [asyncEvent, asyncEvent.GetType().Name];
                }
                else
                {
                    foreach (object[] asyncEventWithSubscribers in AddAsyncEventHandlers(asyncEvent, prepareEventHandlers, exceptionHandlerCount))
                    {
                        yield return asyncEventWithSubscribers;
                    }
                }
            }
        }

        public static IEnumerable<AsyncEventPostHandler<AsyncEventArgs>> CreatePostHandlers(int handlerCount)
        {
            // Generate a anonymous delegate through expressions, since adding
            // the same delegate multiple times will throw an exception
            Expression<Func<AsyncEventArgs, CancellationToken, ValueTask>> postHandlerExpression = (eventArgs, cancellationToken) => ValueTask.CompletedTask;

            for (int i = 0; i < handlerCount; i++)
            {
                yield return postHandlerExpression.Compile().Method.CreateDelegate<AsyncEventPostHandler<AsyncEventArgs>>();
            }
        }

        public static IEnumerable<AsyncEventPreHandler<AsyncEventArgs>> CreatePreHandlers(int handlerCount)
        {
            // Generate a anonymous delegate through expressions, since adding
            // the same delegate multiple times will throw an exception
            Expression<Func<AsyncEventArgs, CancellationToken, ValueTask<bool>>> preHandlerExpression = (eventArgs, cancellationToken) => ValueTask.FromResult(true);

            for (int i = 0; i < handlerCount; i++)
            {
                yield return preHandlerExpression.Compile().Method.CreateDelegate<AsyncEventPreHandler<AsyncEventArgs>>();
            }
        }

        private static IEnumerable<object[]> AddAsyncEventHandlers(IAsyncEvent<AsyncEventArgs> asyncEvent, bool prepareEventHandlers, int exceptionHandlerCount)
        {
            // Generate a anonymous delegate through expressions, since adding
            // the same delegate multiple times will throw an exception
            Expression<Func<AsyncEventArgs, CancellationToken, ValueTask<bool>>> preHandlerExpression = (eventArgs, cancellationToken) => ValueTask.FromResult(true);
            Expression<Func<AsyncEventArgs, CancellationToken, ValueTask>> postHandlerExpression = (eventArgs, cancellationToken) => ValueTask.CompletedTask;

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
                }

                if (exceptionHandlerCount > 0)
                {
                    for (int i = 0; i < exceptionHandlerCount && i < handlerCount; i++)
                    {
                        ThrowHandlers throwHandlers = new();
                        asyncEvent.RemovePreHandler(asyncEvent.PreHandlers[AsyncEventPriority.Normal][i]);
                        asyncEvent.AddPreHandler(throwHandlers.ThrowPreHandler);

                        asyncEvent.RemovePostHandler(asyncEvent.PostHandlers[AsyncEventPriority.Normal][i]);
                        asyncEvent.AddPostHandler(throwHandlers.ThrowPostHandler);
                    }
                }

                if (prepareEventHandlers)
                {
                    asyncEvent.Prepare();
                }

                yield return [asyncEvent, asyncEvent.GetType().Name, (int)handlerCount];
            }
        }

        private class ThrowHandlers
        {
            private static readonly Exception _exception = new();

            public ValueTask<bool> ThrowPreHandler(AsyncEventArgs eventArgs, CancellationToken cancellationToken = default) => throw _exception;
            public ValueTask ThrowPostHandler(AsyncEventArgs eventArgs, CancellationToken cancellationToken = default) => throw _exception;
        }
    }
}
