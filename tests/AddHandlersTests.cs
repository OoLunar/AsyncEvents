using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.AsyncEvents.Tests.Attributes;
using OoLunar.AsyncEvents.Tests.Data;

namespace OoLunar.AsyncEvents.Tests
{
    [TestClass]
    public sealed class AddHandlersTests
    {
        [TestMethod, AsyncEventDataSource]
        public void AddHandlers_SkipsNonHandlers(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddHandlers<NonHandlers>();

            Assert.AreEqual(0, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(0, asyncEvent.PreHandlers.Count);
        }

        [TestMethod, AsyncEventDataSource]
        public void AddHandlers_Instance(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            EventHandlers handlers = new();
            asyncEvent.AddHandlers<EventHandlers>(handlers);

            Assert.AreEqual(1, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers.Count);

            Assert.AreEqual(1, asyncEvent.PostHandlers[AsyncEventPriority.Normal].Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers[AsyncEventPriority.Normal].Count);

            Assert.AreEqual(handlers.PostHandler, asyncEvent.PostHandlers[AsyncEventPriority.Normal][0]);
            Assert.AreEqual(handlers.PreHandler, asyncEvent.PreHandlers[AsyncEventPriority.Normal][0]);
        }

        [TestMethod, AsyncEventDataSource]
        public void AddHandlers_Instance_Twice(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            EventHandlers handlers = new();
            asyncEvent.AddHandlers<EventHandlers>(handlers);
            asyncEvent.AddHandlers<EventHandlers>(handlers);

            Assert.AreEqual(1, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers.Count);

            Assert.AreEqual(1, asyncEvent.PostHandlers[AsyncEventPriority.Normal].Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers[AsyncEventPriority.Normal].Count);

            Assert.AreEqual(handlers.PostHandler, asyncEvent.PostHandlers[AsyncEventPriority.Normal][0]);
            Assert.AreEqual(handlers.PreHandler, asyncEvent.PreHandlers[AsyncEventPriority.Normal][0]);
        }

        [TestMethod, AsyncEventDataSource]
        public void AddHandlers_Instance_WithoutObject(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddHandlers<EventHandlers>();

            Assert.AreEqual(0, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(0, asyncEvent.PreHandlers.Count);
        }

        [TestMethod, AsyncEventDataSource]
        public void AddHandlers_Static(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddHandlers<StaticEventHandlers>();

            Assert.AreEqual(1, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers.Count);

            Assert.AreEqual(1, asyncEvent.PostHandlers[AsyncEventPriority.Normal].Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers[AsyncEventPriority.Normal].Count);

            Assert.AreEqual(StaticEventHandlers.PostHandler, asyncEvent.PostHandlers[AsyncEventPriority.Normal][0]);
            Assert.AreEqual(StaticEventHandlers.PreHandler, asyncEvent.PreHandlers[AsyncEventPriority.Normal][0]);
        }

        [TestMethod, AsyncEventDataSource]
        public void AddHandlers_Static_Twice(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddHandlers<StaticEventHandlers>();
            asyncEvent.AddHandlers<StaticEventHandlers>();

            Assert.AreEqual(1, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers.Count);

            Assert.AreEqual(1, asyncEvent.PostHandlers[AsyncEventPriority.Normal].Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers[AsyncEventPriority.Normal].Count);

            Assert.AreEqual(StaticEventHandlers.PostHandler, asyncEvent.PostHandlers[AsyncEventPriority.Normal][0]);
            Assert.AreEqual(StaticEventHandlers.PreHandler, asyncEvent.PreHandlers[AsyncEventPriority.Normal][0]);
        }
    }
}
