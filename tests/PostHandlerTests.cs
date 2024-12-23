using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoLunar.AsyncEvents.Tests
{
    [TestClass]
    public sealed class PostHandlerTests
    {
        private bool _postHandlerInvoked;

        [TestMethod]
        public async ValueTask InvokeSingleAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();
            serverEvent.AddPostHandler(PostHandlerSetTrueAsync, AsyncEventPriority.Normal);

            await serverEvent.InvokeAsync(new AsyncEventArgs());
            Assert.IsTrue(_postHandlerInvoked);
        }

        [TestMethod]
        public async ValueTask TestHighPriorityAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();
            serverEvent.AddPostHandler(PostHandlerSetTrueAsync, AsyncEventPriority.Highest);
            serverEvent.AddPostHandler(PostHandlerSetFalseAsync, AsyncEventPriority.Normal);

            await serverEvent.InvokeAsync(new AsyncEventArgs());
            Assert.IsFalse(_postHandlerInvoked);
        }

        [TestMethod]
        public async ValueTask TestLowPriorityAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();
            serverEvent.AddPostHandler(PostHandlerSetTrueAsync, AsyncEventPriority.Lowest);
            serverEvent.AddPostHandler(PostHandlerSetFalseAsync, AsyncEventPriority.Normal);

            await serverEvent.InvokeAsync(new AsyncEventArgs());
            Assert.IsTrue(_postHandlerInvoked);
        }

        [TestMethod]
        public async ValueTask TestThrowAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();
            serverEvent.AddPostHandler(PostHandlerSetTrueAsync, AsyncEventPriority.Normal);
            serverEvent.AddPostHandler(PostHandlerThrowAsync, AsyncEventPriority.Highest);

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
            Assert.IsTrue(_postHandlerInvoked);
        }

        [TestMethod]
        public async ValueTask TestThrowMultipleAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();
            serverEvent.AddPostHandler(PostHandlerSetTrueAsync, AsyncEventPriority.Normal);
            serverEvent.AddPostHandler(PostHandlerThrowAsync, AsyncEventPriority.Highest);
            serverEvent.AddPostHandler(PostHandlerThrowAsync2, AsyncEventPriority.Highest);

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
            Assert.IsTrue(_postHandlerInvoked);
        }

        [TestMethod]
        public async ValueTask TestAddAndPrepareAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();
            serverEvent.AddPostHandler(PostHandlerSetTrueAsync, AsyncEventPriority.Normal);

            await serverEvent.InvokeAsync(new AsyncEventArgs());
            Assert.IsTrue(_postHandlerInvoked);

            serverEvent.AddPostHandler(PostHandlerSetFalseAsync, AsyncEventPriority.Lowest);
            serverEvent.Prepare();

            await serverEvent.InvokeAsync(new AsyncEventArgs());
            Assert.IsFalse(_postHandlerInvoked);
        }

        [TestMethod]
        public async ValueTask TestAttributeAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();

            // This should be zero since the methods are not static and need an object reference
            serverEvent.AddHandlers<PostHandlerTests>();
            Assert.AreEqual(0, serverEvent.PreHandlers.Count);
            Assert.AreEqual(0, serverEvent.PostHandlers.Count);

            serverEvent.AddHandlers<PostHandlerTests>(this);
            Assert.AreEqual(0, serverEvent.PreHandlers.Count);
            Assert.AreEqual(2, serverEvent.PostHandlers.Count);

            await serverEvent.InvokeAsync(new AsyncEventArgs());
            Assert.IsFalse(_postHandlerInvoked);
        }

        private ValueTask PostHandlerSetTrueAsync(AsyncEventArgs eventArgs)
        {
            _postHandlerInvoked = true;
            return ValueTask.CompletedTask;
        }

        private ValueTask PostHandlerSetFalseAsync(AsyncEventArgs eventArgs)
        {
            _postHandlerInvoked = false;
            return ValueTask.CompletedTask;
        }

        private ValueTask PostHandlerThrowAsync(AsyncEventArgs eventArgs) => throw new InvalidOperationException();
        private ValueTask PostHandlerThrowAsync2(AsyncEventArgs eventArgs) => throw new InvalidDataException();

        [AsyncEventHandler]
        private ValueTask PostHandlerSetTrueAsyncAttribute(AsyncEventArgs eventArgs)
        {
            _postHandlerInvoked = true;
            return ValueTask.CompletedTask;
        }

        [AsyncEventHandler(AsyncEventPriority.Lowest)]
        private ValueTask PostHandlerSetFalseAsyncAttribute(AsyncEventArgs eventArgs)
        {
            _postHandlerInvoked = false;
            return ValueTask.CompletedTask;
        }
    }
}
