using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.AsyncEvents.Tests.Attributes;
using OoLunar.AsyncEvents.Tests.Data;

namespace OoLunar.AsyncEvents.Tests
{
    [TestClass]
    public sealed class RemovePreHandlerTests
    {
        [TestMethod, AsyncEventDataSource(IncludeDebugEvent = true)]
        public void RemovePreHandler_Instance(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            EventHandlers handlers = new();
            asyncEvent.AddHandlers(handlers);

            Assert.IsTrue(asyncEvent.RemovePreHandler(handlers.PreHandler));
            Assert.AreEqual(0, asyncEvent.PreHandlers.Count);
        }

        [TestMethod, AsyncEventDataSource(IncludeDebugEvent = true)]
        public void RemovePreHandler_Instance_Twice(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            EventHandlers handlers = new();
            asyncEvent.AddHandlers(handlers);

            Assert.IsTrue(asyncEvent.RemovePreHandler(handlers.PreHandler));
            Assert.IsFalse(asyncEvent.RemovePreHandler(handlers.PreHandler));
            Assert.AreEqual(0, asyncEvent.PreHandlers.Count);
        }

        [TestMethod, AsyncEventDataSource(IncludeDebugEvent = true)]
        public void RemovePreHandler_Instance_Priority(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            EventHandlers handlers = new();
            asyncEvent.AddPreHandler(handlers.PreHandler, AsyncEventPriority.High);

            Assert.IsTrue(asyncEvent.RemovePreHandler(handlers.PreHandler, AsyncEventPriority.High));
            Assert.AreEqual(0, asyncEvent.PreHandlers.Count);
        }

        [TestMethod, AsyncEventDataSource(IncludeDebugEvent = true)]
        public void RemovePreHandler_Static(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddPreHandler(StaticEventHandlers.PreHandler);

            Assert.IsTrue(asyncEvent.RemovePreHandler(StaticEventHandlers.PreHandler));
            Assert.AreEqual(0, asyncEvent.PreHandlers.Count);
        }

        [TestMethod, AsyncEventDataSource(IncludeDebugEvent = true)]
        public void RemovePreHandler_Unregistered(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            EventHandlers handlers = new();
            asyncEvent.AddPreHandler(handlers.PreHandler);

            Assert.IsFalse(asyncEvent.RemovePreHandler(StaticEventHandlers.PreHandler));
            Assert.AreEqual(1, asyncEvent.PreHandlers.Count);
        }
    }
}
