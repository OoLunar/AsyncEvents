using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.AsyncEvents.Tests.Data;

namespace OoLunar.AsyncEvents.Tests
{
    [TestClass]
    public sealed class GetHandlerTests
    {
        private readonly AsyncEventContainer _container;

        public GetHandlerTests()
        {
            _container = new();
            _container.AddHandlers<EventHandlers>(new EventHandlers());
        }

        [TestMethod]
        public async ValueTask GetHandler_Generic_MultithreadedAsync()
        {
            AsyncEvent<TestAsyncEventArgs> asyncEvent;
            await Parallel.ForAsync(0, 1000, async (_, _) =>
            {
                asyncEvent = _container.GetAsyncEvent<TestAsyncEventArgs>();
                await asyncEvent.InvokeAsync(new TestAsyncEventArgs());
            });
        }

        [TestMethod]
        public async ValueTask GetHandler_NonGeneric_MultithreadedAsync()
        {
            IAsyncEvent asyncEvent;
            await Parallel.ForAsync(0, 1000, async (_, _) =>
            {
                asyncEvent = _container.GetAsyncEvent(typeof(TestAsyncEventArgs));
                await asyncEvent.InvokeAsync(new TestAsyncEventArgs());
            });
        }
    }
}
