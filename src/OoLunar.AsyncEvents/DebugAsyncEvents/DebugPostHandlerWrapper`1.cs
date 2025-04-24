using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OoLunar.AsyncEvents.DebugAsyncEvents
{
    internal sealed class DebugPostHandlerWrapper<TEventArgs> where TEventArgs : AsyncEventArgs
    {
        public AsyncEventPostHandler<TEventArgs> PostHandler { get; init; }

        private readonly ILogger<DebugAsyncEvent<TEventArgs>> _logger;

        public DebugPostHandlerWrapper(AsyncEventPostHandler<TEventArgs> postHandler, ILogger<DebugAsyncEvent<TEventArgs>> logger)
        {
            PostHandler = postHandler;
            _logger = logger;
        }

        public async ValueTask StartPostHandlerAsync(TEventArgs eventArgs, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Started invoking post-handler '{Handler}'", PostHandler);
            try
            {
                await PostHandler(eventArgs, cancellationToken);
                _logger.LogDebug("Finished invoking post-handler '{Handler}'", PostHandler);
            }
            catch (Exception error)
            {
                _logger.LogError(error, "An exception occurred while invoking post-handler '{Handler}'", PostHandler);
                throw;
            }
        }
    }
}
