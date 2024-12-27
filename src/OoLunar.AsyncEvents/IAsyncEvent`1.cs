using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    /// <summary>
    /// A abstract interface for an asynchronous event.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    public interface IAsyncEvent<in TEventArgs> : IAsyncEvent where TEventArgs : AsyncEventArgs
    {
        /// <inheritdoc cref="AsyncEvent{TEventArgs}.InvokePreHandlersAsync(TEventArgs)"/>
        public ValueTask<bool> InvokePreHandlersAsync(TEventArgs eventArgs);

        /// <inheritdoc cref="AsyncEvent{TEventArgs}.InvokePostHandlersAsync(TEventArgs)"/>
        public ValueTask InvokePostHandlersAsync(TEventArgs eventArgs);

        /// <inheritdoc cref="AsyncEvent{TEventArgs}.InvokeAsync(TEventArgs)"/>
        public async ValueTask<bool> InvokeAsync(TEventArgs eventArgs)
        {
            if (!await InvokePreHandlersAsync(eventArgs))
            {
                return false;
            }

            await InvokePostHandlersAsync(eventArgs);
            return true;
        }

        ValueTask<bool> IAsyncEvent.InvokePreHandlersAsync(AsyncEventArgs eventArgs)
        {
            if (eventArgs is not TEventArgs typedEventArgs)
            {
                typedEventArgs = (TEventArgs)eventArgs;
            }

            return InvokePreHandlersAsync(typedEventArgs);
        }

        ValueTask IAsyncEvent.InvokePostHandlersAsync(AsyncEventArgs eventArgs)
        {
            if (eventArgs is not TEventArgs typedEventArgs)
            {
                typedEventArgs = (TEventArgs)eventArgs;
            }

            return InvokePostHandlersAsync(typedEventArgs);
        }

        ValueTask<bool> IAsyncEvent.InvokeAsync(AsyncEventArgs eventArgs)
        {
            if (eventArgs is not TEventArgs typedEventArgs)
            {
                typedEventArgs = (TEventArgs)eventArgs;
            }

            return InvokeAsync(typedEventArgs);
        }
    }
}
