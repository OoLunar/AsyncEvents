using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    public interface IAsyncEventPostHandler
    {
        public Type EventArgsType { get; }

        public ValueTask InvokeAsync(AsyncEventArgs eventArgs, CancellationToken cancellationToken = default);
    }
}