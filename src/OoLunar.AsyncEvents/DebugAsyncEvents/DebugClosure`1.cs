using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OoLunar.AsyncEvents.DebugAsyncEvents
{
    internal sealed class DebugClosure<TEventArgs> where TEventArgs : AsyncEventArgs
    {
        public AsyncEventPriority Priority { get; init; }

        private readonly ILogger<DebugAsyncEvent<TEventArgs>> _logger;

        public DebugClosure(AsyncEventPriority priority, ILogger<DebugAsyncEvent<TEventArgs>> logger)
        {
            Priority = priority;
            _logger = logger;
        }

        public ValueTask<bool> StartPreHandlerAsync(TEventArgs eventArgs, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Invoking {Priority} priority pre-handlers.", Priority);
            return ValueTask.FromResult(true);
        }

        public ValueTask StartPostHandlerAsync(TEventArgs eventArgs, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Invoking {Priority} priority post-handlers.", Priority);
            return ValueTask.CompletedTask;
        }
    }
}
