using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.AsyncEvents.Tests.Attributes;
using OoLunar.AsyncEvents.Tests.Data;

namespace OoLunar.AsyncEvents.Tests
{
    [TestClass]
    public sealed class ClearHandlers
    {
        [TestMethod, AsyncEventDataSource(IncludeDebugEvent = true)]
        public void Clear_Handlers(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            EventHandlers handlers = new();
            asyncEvent.AddHandlers(handlers);

            asyncEvent.ClearHandlers();

            Assert.AreEqual(0, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(0, asyncEvent.PreHandlers.Count);
        }

        [TestMethod, AsyncEventDataSource(IncludeDebugEvent = true)]
        public void Clear_PreHandlers(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            EventHandlers handlers = new();
            asyncEvent.AddHandlers(handlers);

            asyncEvent.ClearPreHandlers();

            Assert.AreEqual(0, asyncEvent.PreHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PostHandlers.Count);
        }

        [TestMethod, AsyncEventDataSource(IncludeDebugEvent = true)]
        public void Clear_PostHandlers(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            EventHandlers handlers = new();
            asyncEvent.AddHandlers(handlers);

            asyncEvent.ClearPostHandlers();

            Assert.AreEqual(0, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers.Count);
        }
    }
}
