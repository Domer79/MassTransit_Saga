using System;
using System.Threading.Tasks;
using DataBusService.Interfaces;
using MassTransit;
using MassTransit_Saga.Tests.Contracts;

namespace MassTransit_Saga.Tests.MessageHandlers
{
    public class TestMessageHandler: BaseMessageHandler<TestMessage>
    {
        public override Task MessageHandle(TestMessage message, ConsumeContext<TestMessage> context)
        {
            return Console.Out.WriteLineAsync(message.Message);
        }
    }
}