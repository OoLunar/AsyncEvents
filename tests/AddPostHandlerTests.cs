using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.AsyncEvents.Tests.Attributes;
using OoLunar.AsyncEvents.Tests.Data;

namespace OoLunar.AsyncEvents.Tests
{
    [TestClass]
    public sealed class AddPostHandlerTests
    {
        private static ValueTask PostHandler(TestAsyncEventArgs eventArgs) => ValueTask.CompletedTask;

        [TestMethod, AsyncEventDataSource]
        public void AddPostHandler(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddPostHandler(PostHandler);

            Assert.AreEqual(1, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(0, asyncEvent.PreHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PostHandlers[AsyncEventPriority.Normal].Count);
            Assert.AreSame(PostHandler, asyncEvent.PostHandlers[AsyncEventPriority.Normal][0]);
        }

        [TestMethod, AsyncEventDataSource]
        public void AddPostHandler_WithPriority(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddPostHandler(PostHandler, AsyncEventPriority.High);

            Assert.AreEqual(1, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(0, asyncEvent.PreHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PostHandlers[AsyncEventPriority.High].Count);
            Assert.AreSame(PostHandler, asyncEvent.PostHandlers[AsyncEventPriority.High][0]);
        }

        [TestMethod, AsyncEventDataSource]
        public void AddPostHandler_WithPriorityDuplicate(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddPostHandler(PostHandler, AsyncEventPriority.High);
            asyncEvent.AddPostHandler(PostHandler, AsyncEventPriority.Normal);

            Assert.AreEqual(2, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(0, asyncEvent.PreHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PostHandlers[AsyncEventPriority.Normal].Count);
            Assert.AreSame(PostHandler, asyncEvent.PostHandlers[AsyncEventPriority.Normal][0]);
            Assert.AreEqual(1, asyncEvent.PostHandlers[AsyncEventPriority.High].Count);
            Assert.AreSame(PostHandler, asyncEvent.PostHandlers[AsyncEventPriority.High][0]);
        }

        [TestMethod, AsyncEventDataSource]
        public void AddPostHandler_Unique(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddPostHandler(PostHandler);
            asyncEvent.AddPostHandler(PostHandler);

            Assert.AreEqual(1, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(0, asyncEvent.PreHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PostHandlers[AsyncEventPriority.Normal].Count);
            Assert.AreSame(PostHandler, asyncEvent.PostHandlers[AsyncEventPriority.Normal][0]);
        }

        [TestMethod, AsyncEventDataSource]
        public void AddPostHandler_UniqueWithPriority(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddPostHandler(PostHandler, AsyncEventPriority.High);
            asyncEvent.AddPostHandler(PostHandler, AsyncEventPriority.High);

            Assert.AreEqual(1, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(0, asyncEvent.PreHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PostHandlers[AsyncEventPriority.High].Count);
            Assert.AreSame(PostHandler, asyncEvent.PostHandlers[AsyncEventPriority.High][0]);
        }
    }
}
