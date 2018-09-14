using System.Collections.Generic;
using MassTransit;

namespace DataBus.Interfaces
{
    public interface IMessageHandlerBuilder
    {
        IBusFactoryConfigurator Configurator { get; }
        IHost Host { get; }
        void Build();
        IEnumerable<HandlerInfo> GetHandlers();
    }
}