using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OoLunar.AsyncEvents.DebugAsyncEvents
{
    internal sealed class DebugPreHandlerWrapper<TEventArgs> where TEventArgs : AsyncEventArgs
    {
        public AsyncEventPreHandler<TEventArgs> PreHandler { get; init; }

        private readonly ILogger<DebugAsyncEvent<TEventArgs>> _logger;

        public DebugPreHandlerWrapper(AsyncEventPreHandler<TEventArgs> preHandler, ILogger<DebugAsyncEvent<TEventArgs>> logger)
        {
            PreHandler = preHandler;
            _logger = logger;
        }

        public async ValueTask<bool> StartPreHandlerAsync(TEventArgs eventArgs)
        {
            _logger.LogDebug("Started invoking pre-handler '{Handler}'", PreHandler);
            bool result = await PreHandler(eventArgs);
            _logger.LogDebug("Finished invoking pre-handler '{Handler}'", PreHandler);
            return result;
        }
    }
}
