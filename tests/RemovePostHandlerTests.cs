using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.AsyncEvents.Tests.Attributes;
using OoLunar.AsyncEvents.Tests.Data;

namespace OoLunar.AsyncEvents.Tests
{
    [TestClass]
    public sealed class RemovePostHandlerTests
    {
        [TestMethod, AsyncEventDataSource(IncludeDebugEvent = true)]
        public void RemovePostHandler_Instance(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            EventHandlers handlers = new();
            asyncEvent.AddHandlers<EventHandlers>(handlers);

            Assert.IsTrue(asyncEvent.RemovePostHandler(handlers.PostHandler));
            Assert.AreEqual(0, asyncEvent.PostHandlers.Count);
        }

        [TestMethod, AsyncEventDataSource(IncludeDebugEvent = true)]
        public void RemovePostHandler_Instance_Twice(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            EventHandlers handlers = new();
            asyncEvent.AddHandlers<EventHandlers>(handlers);

            Assert.IsTrue(asyncEvent.RemovePostHandler(handlers.PostHandler));
            Assert.IsFalse(asyncEvent.RemovePostHandler(handlers.PostHandler));
            Assert.AreEqual(0, asyncEvent.PostHandlers.Count);
        }

        [TestMethod, AsyncEventDataSource(IncludeDebugEvent = true)]
        public void RemovePostHandler_Instance_Priority(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            EventHandlers handlers = new();
            asyncEvent.AddPostHandler(handlers.PostHandler, AsyncEventPriority.High);

            Assert.IsTrue(asyncEvent.RemovePostHandler(handlers.PostHandler, AsyncEventPriority.High));
            Assert.AreEqual(0, asyncEvent.PostHandlers.Count);
        }

        [TestMethod, AsyncEventDataSource(IncludeDebugEvent = true)]
        public void RemovePostHandler_Static(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddPostHandler(StaticEventHandlers.PostHandler);

            Assert.IsTrue(asyncEvent.RemovePostHandler(StaticEventHandlers.PostHandler));
            Assert.AreEqual(0, asyncEvent.PostHandlers.Count);
        }

        [TestMethod, AsyncEventDataSource(IncludeDebugEvent = true)]
        public void RemovePostHandler_Unregistered(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            EventHandlers handlers = new();
            asyncEvent.AddPostHandler(handlers.PostHandler);

            Assert.IsFalse(asyncEvent.RemovePostHandler(StaticEventHandlers.PostHandler));
            Assert.AreEqual(1, asyncEvent.PostHandlers.Count);
        }
    }
}
