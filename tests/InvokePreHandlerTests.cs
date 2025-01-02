using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.AsyncEvents.Tests.Attributes;
using OoLunar.AsyncEvents.Tests.Data;

namespace OoLunar.AsyncEvents.Tests
{
    [TestClass]
    public sealed class InvokePreHandlerTests
    {
        // InvokePreHandler_StaticAsync
        // InvokePreHandler_InstanceAsync
        // InvokePreHandler_Instance_ThrowsExceptionAsync
        // InvokePreHandler_Instance_TwoHandlersAsync
        // InvokePreHandler_Instance_TwoHandlers_ThrowsAggregateExceptionAsync
        // InvokePreHandler_Instance_TwoHandlers_PriorityAsync
        // InvokePreHandler_Instance_FourHandlers_PriorityAsync
        private static bool _invoked;

        private static ValueTask<bool> PreHandler(TestAsyncEventArgs _)
        {
            _invoked = true;
            return ValueTask.FromResult(true);
        }

        [TestMethod, AsyncEventDataSource]
        public async ValueTask InvokePreHandler_StaticAsync(AsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            _invoked = false;
            asyncEvent.AddPreHandler(PreHandler);

            bool result = await asyncEvent.InvokePreHandlersAsync(new TestAsyncEventArgs());

            Assert.IsTrue(result);
            Assert.IsTrue(_invoked);
        }

        [TestMethod, AsyncEventDataSource]
        public async ValueTask InvokePreHandler_InstanceAsync(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            bool invoked = false;
            asyncEvent.AddPreHandler(_ =>
            {
                invoked = true;
                return ValueTask.FromResult(true);
            });

            bool result = await asyncEvent.InvokePreHandlersAsync(new TestAsyncEventArgs());

            Assert.IsTrue(result);
            Assert.IsTrue(invoked);
        }

        [TestMethod, AsyncEventDataSource]
        public async ValueTask InvokePreHandler_Instance_ThrowsExceptionAsync(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddPreHandler(_ => throw new InvalidOperationException());
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await asyncEvent.InvokePreHandlersAsync(new TestAsyncEventArgs()));
        }

        [TestMethod, AsyncEventDataSource]
        public async ValueTask InvokePreHandler_Instance_TwoHandlersAsync(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            bool invoked1 = false;
            bool invoked2 = false;
            asyncEvent.AddPreHandler(_ =>
            {
                invoked1 = true;
                return ValueTask.FromResult(true);
            });

            asyncEvent.AddPreHandler(_ =>
            {
                invoked2 = true;
                return ValueTask.FromResult(true);
            });

            bool result = await asyncEvent.InvokePreHandlersAsync(new TestAsyncEventArgs());

            Assert.IsTrue(result);
            Assert.IsTrue(invoked1);
            Assert.IsTrue(invoked2);
        }

        [TestMethod, AsyncEventDataSource]
        public async ValueTask InvokePreHandler_Instance_TwoHandlers_ThrowsExceptionAsync(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            bool invoked1 = false;
            asyncEvent.AddPreHandler(_ =>
            {
                invoked1 = true;
                return ValueTask.FromResult(true);
            }, AsyncEventPriority.Normal);

            asyncEvent.AddPreHandler(_ => throw new InvalidOperationException(), AsyncEventPriority.Low);
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await asyncEvent.InvokePreHandlersAsync(new TestAsyncEventArgs()));
            Assert.IsTrue(invoked1);
        }

        [TestMethod, AsyncEventDataSource]
        public async ValueTask InvokePreHandler_Instance_TwoHandlers_ThrowsAggregateExceptionAsync(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddPreHandler(_ => throw new InvalidOperationException(), AsyncEventPriority.Normal);
            asyncEvent.AddPreHandler(_ => throw new InvalidOperationException(), AsyncEventPriority.Low);

            AggregateException aggregateException = await Assert.ThrowsExceptionAsync<AggregateException>(async () => await asyncEvent.InvokePreHandlersAsync(new TestAsyncEventArgs()));
            Assert.AreEqual(2, aggregateException.InnerExceptions.Count);
            Assert.IsInstanceOfType<InvalidOperationException>(aggregateException.InnerExceptions[0]);
            Assert.IsInstanceOfType<InvalidOperationException>(aggregateException.InnerExceptions[1]);
        }

        [TestMethod, AsyncEventDataSource]
        public async ValueTask InvokePreHandler_Instance_TwoHandlers_PriorityAsync(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            bool invoked1 = false;
            bool invoked2 = false;
            asyncEvent.AddPreHandler(_ =>
            {
                invoked1 = true;
                return ValueTask.FromResult(true);
            }, AsyncEventPriority.Normal);

            asyncEvent.AddPreHandler(_ =>
            {
                invoked2 = true;
                return ValueTask.FromResult(true);
            }, AsyncEventPriority.High);

            bool result = await asyncEvent.InvokePreHandlersAsync(new TestAsyncEventArgs());

            Assert.IsTrue(result);
            Assert.IsTrue(invoked1);
            Assert.IsTrue(invoked2);
        }

        [TestMethod, AsyncEventDataSource]
        public async ValueTask InvokePreHandler_Instance_FourHandlers_PriorityAsync(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            bool invoked1 = false;
            bool invoked2 = false;
            bool invoked3 = false;
            bool invoked4 = false;
            asyncEvent.AddPreHandler(_ =>
            {
                invoked1 = true;
                return ValueTask.FromResult(true);
            }, AsyncEventPriority.Low);

            asyncEvent.AddPreHandler(_ =>
            {
                invoked2 = true;
                return ValueTask.FromResult(true);
            }, AsyncEventPriority.Normal);

            asyncEvent.AddPreHandler(_ =>
            {
                invoked3 = true;
                return ValueTask.FromResult(true);
            }, AsyncEventPriority.High);

            asyncEvent.AddPreHandler(_ =>
            {
                invoked4 = true;
                return ValueTask.FromResult(true);
            }, AsyncEventPriority.Highest);

            bool result = await asyncEvent.InvokePreHandlersAsync(new TestAsyncEventArgs());

            Assert.IsTrue(result);
            Assert.IsTrue(invoked1);
            Assert.IsTrue(invoked2);
            Assert.IsTrue(invoked3);
            Assert.IsTrue(invoked4);
        }
    }
}
