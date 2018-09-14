using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit;

namespace DataBus.Interfaces
{
    public interface IExecutionContext
    {
        void SetHandlers(Dictionary<Type, HandlerInfo> messageHandlersDictionary);
        Task Handler<TMessage>(ConsumeContext<TMessage> context) where TMessage : class;
    }
}