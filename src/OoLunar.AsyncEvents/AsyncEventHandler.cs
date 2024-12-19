using System.Threading.Tasks;

namespace OoLunar.AsyncEvents
{
    public delegate ValueTask AsyncEventHandler(AsyncEventArgs eventArgs);
    public delegate ValueTask AsyncEventHandler<T>(T eventArgs) where T : AsyncEventArgs;
}
