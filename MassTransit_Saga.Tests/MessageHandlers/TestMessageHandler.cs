using System;
using System.Threading.Tasks;
using DataBusService.Interfaces;
using MassTransit_Saga.Tests.Contracts;

namespace MassTransit_Saga.Tests.MessageHandlers
{
    public class TestMessageHandler: BaseMessageHandler<TestMessage>
    {
        public override Task MessageHandle(TestMessage message)
        {
            return Console.Out.WriteLineAsync(message.Message);
        }
    }
}