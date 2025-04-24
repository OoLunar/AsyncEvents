using System.Threading;
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
        /// <summary>
        /// Removes all pre-handlers from the event.
        /// </summary>
        public void ClearPreHandlers();

        /// <summary>
        /// Removes all post-handlers from the event.
        /// </summary>
        public void ClearPostHandlers();

        /// <summary>
        /// Removes all pre/post-handlers from the event.
        /// </summary>
        public void ClearHandlers()
        {
            ClearPreHandlers();
            ClearPostHandlers();
        }

        /// <summary>
        /// Efficiently prepares the event for invocation by compiling the pre/post-handlers into a single delegate.
        /// </summary>
        public void Prepare();

        /// <summary>
        /// Efficiently prepares the event for invocation by compiling the pre/post-handlers into a single delegate.
        /// </summary>
        public ValueTask PrepareAsync(CancellationToken cancellationToken = default)
        {
            Prepare();
            return default;
        }

        /// <inheritdoc cref="IAsyncEvent{TEventArgs}.InvokePreHandlersAsync(TEventArgs, CancellationToken)"/>
        public ValueTask<bool> InvokePreHandlersAsync(AsyncEventArgs eventArgs, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="IAsyncEvent{TEventArgs}.InvokePostHandlersAsync(TEventArgs, CancellationToken)"/>
        public ValueTask InvokePostHandlersAsync(AsyncEventArgs eventArgs, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="IAsyncEvent{TEventArgs}.InvokeAsync(TEventArgs, CancellationToken)"/>
        public async ValueTask<bool> InvokeAsync(AsyncEventArgs eventArgs, CancellationToken cancellationToken = default)
        {
            if (!await InvokePreHandlersAsync(eventArgs, cancellationToken))
            {
                return false;
            }

            await InvokePostHandlersAsync(eventArgs, cancellationToken);
            return true;
        }
    }
}
