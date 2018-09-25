using System;
using System.Threading.Tasks;

namespace DataBusService
{
    public interface IDatabusSynchronizationContext: IDisposable
    {
        Task MessageHandle(object message);
        void Wait(Func<object, Task> messageHandler);
    }
}