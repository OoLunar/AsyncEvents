using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    /// <summary>
    /// A non-generic version of <see cref="IAsyncEvent{TEventArgs}"/>. This interface is used to allow
    /// the invocation of an <see cref="IAsyncEvent{TEventArgs}"/> without knowing the type of the event
    /// arguments. The main purpose of this interface is to allow the invocation of an <see cref="IAsyncEvent{TEventArgs}"/>
    /// through a non-generic interface, which is useful when the type of the event arguments is not known
    /// at compile-time, but may be discovered at runtime through reflection or other similar means.
    /// </summary>
    /// <remarks>
    /// The event arguments passed to the methods of this interface will be casted to the type of the
    /// event arguments of the <see cref="IAsyncEvent{TEventArgs}"/> implementation. Please ensure that
    /// the event arguments passed to the methods of this interface are of the correct type.
    /// </remarks>
    public interface IAsyncEvent
    {
        /// <inheritdoc cref="AsyncEvent{TEventArgs}.InvokePreHandlersAsync(TEventArgs)"/>
        public ValueTask<bool> InvokePreHandlersAsync(AsyncEventArgs eventArgs);

        /// <inheritdoc cref="AsyncEvent{TEventArgs}.InvokePostHandlersAsync(TEventArgs)"/>
        public ValueTask InvokePostHandlersAsync(AsyncEventArgs eventArgs);

        /// <inheritdoc cref="AsyncEvent{TEventArgs}.InvokeAsync(TEventArgs)"/>
        public async ValueTask<bool> InvokeAsync(AsyncEventArgs eventArgs)
        {
            if (!await InvokePreHandlersAsync(eventArgs))
            {
                return false;
            }

            await InvokePostHandlersAsync(eventArgs);
            return true;
        }
    }
}
