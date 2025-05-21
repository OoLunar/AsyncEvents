using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.AsyncEvents.Tests.Attributes;
using OoLunar.AsyncEvents.Tests.Data;

namespace OoLunar.AsyncEvents.Tests
{
    [TestClass]
    public sealed class AddHandlersTests
    {
        [TestMethod, AsyncEventDataSource]
        public async ValueTask AddHandlers_MultithreadedAsync(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            EventHandlers handlers = new();
            await Parallel.ForAsync(0, 1000, (_, _) =>
            {
                asyncEvent.AddHandlers(handlers);
                return ValueTask.CompletedTask;
            });

            Assert.AreEqual(1, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers.Count);

            Assert.AreEqual(1, asyncEvent.PostHandlers[AsyncEventPriority.Normal].Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers[AsyncEventPriority.Normal].Count);

            Assert.AreEqual(handlers.InvokeAsync, asyncEvent.PostHandlers[AsyncEventPriority.Normal][0]);
            Assert.AreEqual(handlers.PreInvokeAsync, asyncEvent.PreHandlers[AsyncEventPriority.Normal][0]);
        }

        [TestMethod, AsyncEventDataSource]
        public void AddHandlers_SkipsNonHandlers(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddHandlers(new NonHandlers());

            Assert.AreEqual(0, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(0, asyncEvent.PreHandlers.Count);
        }

        [TestMethod, AsyncEventDataSource]
        public void AddHandlers_Instance(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            EventHandlers handlers = new();
            asyncEvent.AddHandlers(handlers);

            Assert.AreEqual(1, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers.Count);

            Assert.AreEqual(1, asyncEvent.PostHandlers[AsyncEventPriority.Normal].Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers[AsyncEventPriority.Normal].Count);

            Assert.AreEqual(handlers.InvokeAsync, asyncEvent.PostHandlers[AsyncEventPriority.Normal][0]);
            Assert.AreEqual(handlers.PreInvokeAsync, asyncEvent.PreHandlers[AsyncEventPriority.Normal][0]);
        }

        [TestMethod, AsyncEventDataSource]
        public void AddHandlers_Instance_Twice(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            EventHandlers handlers = new();
            asyncEvent.AddHandlers(handlers);
            asyncEvent.AddHandlers(handlers);

            Assert.AreEqual(1, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers.Count);

            Assert.AreEqual(1, asyncEvent.PostHandlers[AsyncEventPriority.Normal].Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers[AsyncEventPriority.Normal].Count);

            Assert.AreEqual(handlers.InvokeAsync, asyncEvent.PostHandlers[AsyncEventPriority.Normal][0]);
            Assert.AreEqual(handlers.PreInvokeAsync, asyncEvent.PreHandlers[AsyncEventPriority.Normal][0]);
        }

        [TestMethod, AsyncEventDataSource]
        public void AddHandlers_Static(IAsyncEvent<TestAsyncEventArgs> asyncEvent)
        {
            asyncEvent.AddPostHandler(StaticEventHandlers.PostHandler);
            asyncEvent.AddPreHandler(StaticEventHandlers.PreHandler);

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
            asyncEvent.AddPostHandler(StaticEventHandlers.PostHandler);
            asyncEvent.AddPostHandler(StaticEventHandlers.PostHandler);
            asyncEvent.AddPreHandler(StaticEventHandlers.PreHandler);
            asyncEvent.AddPreHandler(StaticEventHandlers.PreHandler);

            Assert.AreEqual(1, asyncEvent.PostHandlers.Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers.Count);

            Assert.AreEqual(1, asyncEvent.PostHandlers[AsyncEventPriority.Normal].Count);
            Assert.AreEqual(1, asyncEvent.PreHandlers[AsyncEventPriority.Normal].Count);

            Assert.AreEqual(StaticEventHandlers.PostHandler, asyncEvent.PostHandlers[AsyncEventPriority.Normal][0]);
            Assert.AreEqual(StaticEventHandlers.PreHandler, asyncEvent.PreHandlers[AsyncEventPriority.Normal][0]);
        }
    }
}
