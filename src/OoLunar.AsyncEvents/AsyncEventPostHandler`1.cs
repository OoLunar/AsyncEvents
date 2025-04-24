using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    /// <summary>
    /// Represents a method/delegate that handles an asynchronous event.
    /// </summary>
    /// <param name="eventArgs">The event arguments.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public delegate ValueTask AsyncEventPostHandler<T>(T eventArgs, CancellationToken cancellationToken = default) where T : AsyncEventArgs;
}
