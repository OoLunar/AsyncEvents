using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoLunar.AsyncEvents.Tests
{
    [TestClass]
    public class AsyncEventContainerTests
    {
        private readonly AsyncEventContainer _container;

        public AsyncEventContainerTests() => _container = new AsyncEventContainer();

        [TestMethod]
        public void Constructor_DefaultValues_ShouldBeSet()
        {
            Assert.IsFalse(_container.ParallelizationEnabled);
            Assert.AreEqual(0, _container.MinimumParallelHandlerCount);
        }

        [TestMethod]
        public void Constructor_ParallelizationEnabled_ShouldBeSet()
        {
            AsyncEventContainer container = new(true);
            Assert.IsTrue(container.ParallelizationEnabled);
            Assert.AreEqual(Environment.ProcessorCount, container.MinimumParallelHandlerCount);
        }

        [TestMethod]
        public void Constructor_CustomParallelization_ShouldBeSet()
        {
            AsyncEventContainer container = new(true, 5);
            Assert.IsTrue(container.ParallelizationEnabled);
            Assert.AreEqual(5, container.MinimumParallelHandlerCount);
        }

        [TestMethod]
        public void GetAsyncEvent_ShouldReturnPreparedAsyncEvent()
        {
            AsyncEvent<AsyncEventArgs> asyncEvent = _container.GetAsyncEvent<AsyncEventArgs>();
            Assert.AreEqual(0, asyncEvent.PreHandlers.Count);
            Assert.AreEqual(0, asyncEvent.PostHandlers.Count);
        }

        [TestMethod]
        public void GetAsyncEvent_NonGeneric_ShouldReturnPreparedAsyncEvent()
        {
            Type asyncEventArgsType = typeof(AsyncEventArgs);
            IAsyncEvent asyncEvent = _container.GetAsyncEvent(asyncEventArgsType);
            AsyncEvent<AsyncEventArgs> asyncEventGeneric = _container.GetAsyncEvent<AsyncEventArgs>();
            Assert.AreEqual(asyncEvent, asyncEventGeneric);
        }

        [TestMethod]
        public void AddPreHandler_ShouldRegisterHandler()
        {
            static ValueTask<bool> handler(AsyncEventArgs args) => new(true);
            _container.AddPreHandler((AsyncEventPreHandler<AsyncEventArgs>)handler, AsyncEventPriority.Normal);

            AsyncEvent<AsyncEventArgs> asyncEvent = _container.GetAsyncEvent<AsyncEventArgs>();
            Assert.IsTrue(asyncEvent.PreHandlers.ContainsKey(handler));
        }

        [TestMethod]
        public void AddPostHandler_ShouldRegisterHandler()
        {
            static ValueTask handler(AsyncEventArgs args) => default;
            _container.AddPostHandler((AsyncEventPostHandler<AsyncEventArgs>)handler, AsyncEventPriority.Normal);

            AsyncEvent<AsyncEventArgs> asyncEvent = _container.GetAsyncEvent<AsyncEventArgs>();
            Assert.IsTrue(asyncEvent.PostHandlers.ContainsKey(handler));
        }

        [TestMethod]
        public void ClearPreHandlers_ShouldRemoveAllPreHandlers()
        {
            static ValueTask<bool> handler(AsyncEventArgs args) => new(true);
            _container.AddPreHandler((AsyncEventPreHandler<AsyncEventArgs>)handler, AsyncEventPriority.Normal);
            _container.ClearPreHandlers<AsyncEventArgs>();

            AsyncEvent<AsyncEventArgs> asyncEvent = _container.GetAsyncEvent<AsyncEventArgs>();
            Assert.AreEqual(0, asyncEvent.PreHandlers.Count);
        }

        [TestMethod]
        public void ClearPostHandlers_ShouldRemoveAllPostHandlers()
        {
            static ValueTask handler(AsyncEventArgs args) => default;
            _container.AddPostHandler((AsyncEventPostHandler<AsyncEventArgs>)handler, AsyncEventPriority.Normal);
            _container.ClearPostHandlers<AsyncEventArgs>();

            AsyncEvent<AsyncEventArgs> asyncEvent = _container.GetAsyncEvent<AsyncEventArgs>();
            Assert.AreEqual(0, asyncEvent.PostHandlers.Count);
        }

        [TestMethod]
        public void Prepare_ShouldRecompileHandlers()
        {
            static ValueTask<bool> preHandler(AsyncEventArgs args) => new(true);
            static ValueTask postHandler(AsyncEventArgs args) => default;

            _container.AddPreHandler((AsyncEventPreHandler<AsyncEventArgs>)preHandler, AsyncEventPriority.High);
            _container.AddPostHandler((AsyncEventPostHandler<AsyncEventArgs>)postHandler, AsyncEventPriority.Low);
            _container.Prepare<AsyncEventArgs>();

            AsyncEvent<AsyncEventArgs> asyncEvent = _container.GetAsyncEvent<AsyncEventArgs>();
            Assert.IsTrue(asyncEvent.PreHandlers.ContainsKey(preHandler));
            Assert.IsTrue(asyncEvent.PostHandlers.ContainsKey(postHandler));
        }
    }
}
