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

        public ValueTask<bool> StartPreHandlerAsync(TEventArgs eventArgs)
        {
            _logger.LogDebug("Started invoking {Priority} priority pre-handlers.", Priority);
            return ValueTask.FromResult(true);
        }

        public ValueTask StartPostHandlerAsync(TEventArgs eventArgs)
        {
            _logger.LogDebug("Started invoking {Priority} priority post-handlers.", Priority);
            return ValueTask.CompletedTask;
        }
    }
}
