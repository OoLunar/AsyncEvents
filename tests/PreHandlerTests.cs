using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoLunar.AsyncEvents.Tests
{
    [TestClass]
    public sealed class PreHandlerTests
    {
        private bool _preHandlerInvoked;

        [TestMethod]
        public async ValueTask InvokeSingleAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();
            serverEvent.AddPreHandler(PreHandlerSetTrueAsync, AsyncEventPriority.Normal);

            bool result = await serverEvent.InvokeAsync(new AsyncEventArgs());
            Assert.IsTrue(_preHandlerInvoked);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async ValueTask TestHighPriorityAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();
            serverEvent.AddPreHandler(PreHandlerSetFalseAsync, AsyncEventPriority.Normal);
            serverEvent.AddPreHandler(PreHandlerSetTrueAsync, AsyncEventPriority.Highest);

            bool result = await serverEvent.InvokeAsync(new AsyncEventArgs());
            Assert.IsTrue(_preHandlerInvoked);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async ValueTask TestLowPriorityAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();
            serverEvent.AddPreHandler(PreHandlerSetTrueAsync, AsyncEventPriority.Lowest);
            serverEvent.AddPreHandler(PreHandlerSetFalseAsync, AsyncEventPriority.Normal);

            bool result = await serverEvent.InvokeAsync(new AsyncEventArgs());
            Assert.IsFalse(_preHandlerInvoked);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async ValueTask TestFalseAsync()
        {
            AsyncEvent<AsyncEventArgs> serverEvent = new();
            serverEvent.AddPreHandler(PreHandlerSetTrueAsync, AsyncEventPriority.Normal);
            serverEvent.AddPreHandler(PreHandlerReturnFalseAsync, AsyncEventPriority.Highest);

            bool result = await serverEvent.InvokeAsync(new AsyncEventArgs());
            Assert.IsTrue(_preHandlerInvoked);
            Assert.IsFalse(result);
        }

        private ValueTask<bool> PreHandlerSetTrueAsync(AsyncEventArgs eventArgs)
        {
            _preHandlerInvoked = true;
            return ValueTask.FromResult(true);
        }

        private ValueTask<bool> PreHandlerSetFalseAsync(AsyncEventArgs eventArgs)
        {
            _preHandlerInvoked = false;
            return ValueTask.FromResult(true);
        }

        private ValueTask<bool> PreHandlerReturnFalseAsync(AsyncEventArgs eventArgs) => ValueTask.FromResult(false);
    }
}
