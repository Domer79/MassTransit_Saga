using System.Threading.Tasks;

namespace DataBus.Interfaces
{
    public abstract class MessageHandler<TMessage>
        where TMessage : class
    {
        public abstract Task MessageHandle(TMessage message);
    }
}