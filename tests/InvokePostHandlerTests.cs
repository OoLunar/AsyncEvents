using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.AsyncEvents.Tests.Attributes;
using OoLunar.AsyncEvents.Tests.Data;

namespace OoLunar.AsyncEvents.Tests
{
    [TestClass]
    public sealed class InvokePostHandlerTests
    {
        // InvokePostHandler_StaticAsync
        // InvokePostHandler_InstanceAsync
        // InvokePostHandler_Instance_ThrowsExceptionAsync
        // InvokePostHandler_Instance_TwoHandlersAsync
        // InvokePostHandler_Instance_TwoHandlers_ThrowsAggregateExceptionAsync
        // InvokePostHandler_Instance_TwoHandlers_PriorityAsync
        // InvokePostHandler_Instance_FourHandlers_PriorityAsync
        private static bool _invoked;

        private static ValueTask PostHandler(TestAsyncEventArgs _, CancellationToken cancellationToken)
        {
            _invoked = true;
            return ValueTask.CompletedTask;
        }

        [TestMethod, AsyncEventDataSource]
        public async ValueTask InvokePostHandler_StaticAsync(AsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            _invoked = false;
            asyncEvent.AddPostHandler(PostHandler);

            await asyncEvent.InvokePostHandlersAsync(new TestAsyncEventArgs());

            Assert.IsTrue(_invoked);
        }

        [TestMethod, AsyncEventDataSource]
        public async ValueTask InvokePostHandler_InstanceAsync(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            bool invoked = false;
            asyncEvent.AddPostHandler((_, _) =>
            {
                invoked = true;
                return ValueTask.CompletedTask;
            });

            await asyncEvent.InvokePostHandlersAsync(new TestAsyncEventArgs());

            Assert.IsTrue(invoked);
        }

        [TestMethod, AsyncEventDataSource]
        public async ValueTask InvokePostHandler_Instance_ThrowsExceptionAsync(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddPostHandler((_, _) => throw new InvalidOperationException());
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await asyncEvent.InvokePostHandlersAsync(new TestAsyncEventArgs()));
        }

        [TestMethod, AsyncEventDataSource]
        public async ValueTask InvokePostHandler_Instance_TwoHandlersAsync(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            bool invoked1 = false;
            bool invoked2 = false;
            asyncEvent.AddPostHandler((_, _) =>
            {
                invoked1 = true;
                return ValueTask.CompletedTask;
            });

            asyncEvent.AddPostHandler((_, _) =>
            {
                invoked2 = true;
                return ValueTask.CompletedTask;
            });

            await asyncEvent.InvokePostHandlersAsync(new TestAsyncEventArgs());

            Assert.IsTrue(invoked1);
            Assert.IsTrue(invoked2);
        }

        [TestMethod, AsyncEventDataSource]
        public async ValueTask InvokePostHandler_Instance_TwoHandlers_ThrowsExceptionAsync(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            bool invoked1 = false;
            asyncEvent.AddPostHandler((_, _) =>
            {
                invoked1 = true;
                return ValueTask.CompletedTask;
            }, AsyncEventPriority.Normal);

            asyncEvent.AddPostHandler((_, _) => throw new InvalidOperationException(), AsyncEventPriority.Low);
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await asyncEvent.InvokePostHandlersAsync(new TestAsyncEventArgs()));
            Assert.IsTrue(invoked1);
        }

        [TestMethod, AsyncEventDataSource]
        public async ValueTask InvokePostHandler_Instance_TwoHandlers_ThrowsAggregateExceptionAsync(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddPostHandler((_, _) => throw new InvalidOperationException(), AsyncEventPriority.Normal);
            asyncEvent.AddPostHandler((_, _) => throw new InvalidOperationException(), AsyncEventPriority.Low);

            AggregateException aggregateException = await Assert.ThrowsExceptionAsync<AggregateException>(async () => await asyncEvent.InvokePostHandlersAsync(new TestAsyncEventArgs()));
            Assert.AreEqual(2, aggregateException.InnerExceptions.Count);
            Assert.IsInstanceOfType<InvalidOperationException>(aggregateException.InnerExceptions[0]);
            Assert.IsInstanceOfType<InvalidOperationException>(aggregateException.InnerExceptions[1]);
        }

        [TestMethod, AsyncEventDataSource]
        public async ValueTask InvokePostHandler_Instance_TwoHandlers_PriorityAsync(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            bool invoked1 = false;
            bool invoked2 = false;
            asyncEvent.AddPostHandler((_, _) =>
            {
                invoked1 = true;
                return ValueTask.CompletedTask;
            }, AsyncEventPriority.Normal);

            asyncEvent.AddPostHandler((_, _) =>
            {
                invoked2 = true;
                return ValueTask.CompletedTask;
            }, AsyncEventPriority.High);

            await asyncEvent.InvokePostHandlersAsync(new TestAsyncEventArgs());

            Assert.IsTrue(invoked1);
            Assert.IsTrue(invoked2);
        }

        [TestMethod, AsyncEventDataSource]
        public async ValueTask InvokePostHandler_Instance_FourHandlers_PriorityAsync(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            bool invoked1 = false;
            bool invoked2 = false;
            bool invoked3 = false;
            bool invoked4 = false;
            asyncEvent.AddPostHandler((_, _) =>
            {
                invoked1 = true;
                return ValueTask.CompletedTask;
            }, AsyncEventPriority.Low);

            asyncEvent.AddPostHandler((_, _) =>
            {
                invoked2 = true;
                return ValueTask.CompletedTask;
            }, AsyncEventPriority.Normal);

            asyncEvent.AddPostHandler((_, _) =>
            {
                invoked3 = true;
                return ValueTask.CompletedTask;
            }, AsyncEventPriority.High);

            asyncEvent.AddPostHandler((_, _) =>
            {
                invoked4 = true;
                return ValueTask.CompletedTask;
            }, AsyncEventPriority.Highest);

            await asyncEvent.InvokePostHandlersAsync(new TestAsyncEventArgs());

            Assert.IsTrue(invoked1);
            Assert.IsTrue(invoked2);
            Assert.IsTrue(invoked3);
            Assert.IsTrue(invoked4);
        }
    }
}
