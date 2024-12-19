using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    public delegate ValueTask<bool> AsyncEventPreHandler(AsyncEventArgs eventArgs);
    public delegate ValueTask<bool> AsyncEventPreHandler<T>(T eventArgs) where T : AsyncEventArgs;
}
