using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBusService.Interfaces;
using MassTransit_Saga.Tests.Contracts;

namespace MassTransit_Saga.Tests.MessageHandlers
{
    public class Test2MessageHandler: BaseMessageHandler<TestMessage>
    {
        private readonly IInterface1 _implement1;
        private readonly IInterface2 _implement2;

        public Test2MessageHandler(IInterface1 implement1, IInterface2 implement2)
        {
            _implement1 = implement1;
            _implement2 = implement2;
        }

        public override async Task MessageHandle(TestMessage message)
        {
            await Console.Out.WriteLineAsync(message.Message);

        }
    }

    public interface IInterface1
    {
        string InstanceName { get; }
    }

    public interface IInterface2
    {
        string InstanceName { get; }
    }

    public class Interface1Implement : IInterface1
    {
        public string InstanceName => "Instance1";
    }

    public class Interface2Implement : IInterface2
    {
        public string InstanceName => "Instance2";
    }
}
