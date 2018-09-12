using System.Collections.Generic;
using MassTransit;

namespace DataBus.Interfaces
{
    public interface IMessageHandlerConfigurator
    {
        IBusFactoryConfigurator Configurator { get; }
        IHost Host { get; }
        void Build();
        IEnumerable<HandlerInfo> GetHandlers();
    }
}