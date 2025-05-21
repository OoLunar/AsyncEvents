using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    public interface IAsyncEventPreHandler
    {
        public Type EventArgsType { get; }

        public ValueTask<bool> PreInvokeAsync(AsyncEventArgs eventArgs, CancellationToken cancellationToken = default);
    }
}
