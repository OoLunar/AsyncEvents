using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.AsyncEvents.Tests.Attributes;
using OoLunar.AsyncEvents.Tests.Data;

namespace OoLunar.AsyncEvents.Tests
{
    [TestClass]
    public sealed class AddPreHandlerTests
    {
        private static ValueTask<bool> PreHandler(TestAsyncEventArgs eventArgs, CancellationToken cancellationToken) => ValueTask.FromResult(true);

        [TestMethod, AsyncEventDataSource]
        public void AddPreHandler(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddPreHandler(PreHandler);

            Assert.AreEqual(0, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers[AsyncEventPriority.Normal].Count);
            Assert.AreSame(PreHandler, asyncEvent.PreHandlers[AsyncEventPriority.Normal][0]);
        }

        [TestMethod, AsyncEventDataSource]
        public void AddPreHandler_WithPriority(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddPreHandler(PreHandler, AsyncEventPriority.High);

            Assert.AreEqual(0, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers[AsyncEventPriority.High].Count);
            Assert.AreSame(PreHandler, asyncEvent.PreHandlers[AsyncEventPriority.High][0]);
        }

        [TestMethod, AsyncEventDataSource]
        public void AddPreHandler_WithPriorityDuplicate(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddPreHandler(PreHandler, AsyncEventPriority.High);
            asyncEvent.AddPreHandler(PreHandler, AsyncEventPriority.Normal);

            Assert.AreEqual(0, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(2, asyncEvent.PreHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers[AsyncEventPriority.Normal].Count);
            Assert.AreSame(PreHandler, asyncEvent.PreHandlers[AsyncEventPriority.Normal][0]);
            Assert.AreEqual(1, asyncEvent.PreHandlers[AsyncEventPriority.High].Count);
            Assert.AreSame(PreHandler, asyncEvent.PreHandlers[AsyncEventPriority.High][0]);
        }

        [TestMethod, AsyncEventDataSource]
        public void AddPreHandler_Unique(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddPreHandler(PreHandler);
            asyncEvent.AddPreHandler(PreHandler);

            Assert.AreEqual(0, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers[AsyncEventPriority.Normal].Count);
            Assert.AreSame(PreHandler, asyncEvent.PreHandlers[AsyncEventPriority.Normal][0]);
        }

        [TestMethod, AsyncEventDataSource]
        public void AddPreHandler_UniqueWithPriority(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddPreHandler(PreHandler, AsyncEventPriority.High);
            asyncEvent.AddPreHandler(PreHandler, AsyncEventPriority.High);

            Assert.AreEqual(0, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers[AsyncEventPriority.High].Count);
            Assert.AreSame(PreHandler, asyncEvent.PreHandlers[AsyncEventPriority.High][0]);
        }
    }
}
