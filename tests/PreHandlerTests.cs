using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoLunar.AsyncEvents.Tests
{
    [TestClass]
    public sealed class PreHandlerTests
    {
        private bool _preHandlerInvoked;

        [TestMethod]
        public async ValueTask InvokeSingleAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();
            serverEvent.AddPreHandler(PreHandlerSetTrueAsync, AsyncEventPriority.Normal);

            bool result = await serverEvent.InvokeAsync(new AsyncEventArgs());
            Assert.IsTrue(_preHandlerInvoked);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async ValueTask TestHighPriorityAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();
            serverEvent.AddPreHandler(PreHandlerSetTrueAsync, AsyncEventPriority.Highest);
            serverEvent.AddPreHandler(PreHandlerSetFalseAsync, AsyncEventPriority.Normal);

            bool result = await serverEvent.InvokeAsync(new AsyncEventArgs());
            Assert.IsFalse(_preHandlerInvoked);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async ValueTask TestLowPriorityAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();
            serverEvent.AddPreHandler(PreHandlerSetTrueAsync, AsyncEventPriority.Lowest);
            serverEvent.AddPreHandler(PreHandlerSetFalseAsync, AsyncEventPriority.Normal);

            bool result = await serverEvent.InvokeAsync(new AsyncEventArgs());
            Assert.IsTrue(_preHandlerInvoked);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async ValueTask TestFalseAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();
            serverEvent.AddPreHandler(PreHandlerSetTrueAsync, AsyncEventPriority.Normal);
            serverEvent.AddPreHandler(PreHandlerReturnFalseAsync, AsyncEventPriority.Highest);

            bool result = await serverEvent.InvokeAsync(new AsyncEventArgs());
            Assert.IsTrue(_preHandlerInvoked);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async ValueTask TestThrowAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();
            serverEvent.AddPreHandler(PreHandlerSetTrueAsync, AsyncEventPriority.Normal);
            serverEvent.AddPreHandler(PreHandlerThrowAsync, AsyncEventPriority.Highest);

            bool caught = false;
            try
            {
                await serverEvent.InvokeAsync(new AsyncEventArgs());
            }
            catch (InvalidOperationException error)
            {
                caught = true;
                Assert.IsInstanceOfType<InvalidOperationException>(error);
            }

            Assert.IsTrue(caught);
            Assert.IsTrue(_preHandlerInvoked);
        }

        [TestMethod]
        public async ValueTask TestThrowMultipleAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();
            serverEvent.AddPreHandler(PreHandlerSetTrueAsync, AsyncEventPriority.Normal);
            serverEvent.AddPreHandler(PreHandlerThrowAsync, AsyncEventPriority.Highest);
            serverEvent.AddPreHandler(PreHandlerThrowAsync2, AsyncEventPriority.Highest);

            bool caught = false;
            try
            {
                await serverEvent.InvokeAsync(new AsyncEventArgs());
            }
            catch (AggregateException error)
            {
                caught = true;
                Assert.IsInstanceOfType<AggregateException>(error);
                Assert.AreEqual(2, error.InnerExceptions.Count);
                Assert.IsInstanceOfType<InvalidDataException>(error.InnerExceptions[0]);
                Assert.IsInstanceOfType<InvalidOperationException>(error.InnerExceptions[1]);
            }

            Assert.IsTrue(caught);
            Assert.IsTrue(_preHandlerInvoked);
        }

        [TestMethod]
        public async ValueTask TestAddAndPrepareAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();
            serverEvent.AddPreHandler(PreHandlerSetTrueAsync, AsyncEventPriority.Normal);

            bool result = await serverEvent.InvokeAsync(new AsyncEventArgs());
            Assert.IsTrue(result);
            Assert.IsTrue(_preHandlerInvoked);

            serverEvent.AddPreHandler(PreHandlerSetFalseAsync, AsyncEventPriority.Lowest);
            serverEvent.Prepare();

            result = await serverEvent.InvokeAsync(new AsyncEventArgs());
            Assert.IsTrue(result);
            Assert.IsFalse(_preHandlerInvoked);
        }

        [TestMethod]
        public async ValueTask TestAttributeAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();

            // This should be zero since the methods are not static and need an object reference
            serverEvent.AddHandlers<PreHandlerTests>();
            Assert.AreEqual(0, serverEvent.PreHandlers.Count);
            Assert.AreEqual(0, serverEvent.PostHandlers.Count);

            serverEvent.AddHandlers<PreHandlerTests>(this);
            Assert.AreEqual(2, serverEvent.PreHandlers.Count);
            Assert.AreEqual(0, serverEvent.PostHandlers.Count);

            bool result = await serverEvent.InvokeAsync(new AsyncEventArgs());
            Assert.IsTrue(result);
            Assert.IsFalse(_preHandlerInvoked);
        }

        private ValueTask<bool> PreHandlerSetTrueAsync(AsyncEventArgs eventArgs)
        {
            _preHandlerInvoked = true;
            return ValueTask.FromResult(true);
        }

        private ValueTask<bool> PreHandlerSetFalseAsync(AsyncEventArgs eventArgs)
        {
            _preHandlerInvoked = false;
            return ValueTask.FromResult(true);
        }

        private ValueTask<bool> PreHandlerReturnFalseAsync(AsyncEventArgs eventArgs) => ValueTask.FromResult(false);
        private ValueTask<bool> PreHandlerThrowAsync(AsyncEventArgs eventArgs) => throw new InvalidOperationException();
        private ValueTask<bool> PreHandlerThrowAsync2(AsyncEventArgs eventArgs) => throw new InvalidDataException();

        [AsyncEventHandler]
        private ValueTask<bool> PreHandlerSetTrueAsyncAttribute(AsyncEventArgs eventArgs)
        {
            _preHandlerInvoked = true;
            return ValueTask.FromResult(true);
        }

        [AsyncEventHandler(AsyncEventPriority.Lowest)]
        private ValueTask<bool> PreHandlerSetFalseAsyncAttribute(AsyncEventArgs eventArgs)
        {
            _preHandlerInvoked = false;
            return ValueTask.FromResult(true);
        }
    }
}
