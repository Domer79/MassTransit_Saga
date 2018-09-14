using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataBus;
using DataBus.Interfaces;
using MassTransit_Saga.Contracts;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace MassTransit_Saga.Tests
{
    [TestFixture()]
    public class DataBusTests
    {
        [Test]
        public void ReceiveMessage_Test()
        {
            using (var bus = new Bus())
            {
                var executionContext = new DataBus.TestExecutionContext(bus);
                DatabusExecutionContext.SetExecutionContext(executionContext);
                bus.Start();
                executionContext.Publish<TestMessage>(new {Message = "Hello Test World!"}).Wait(message =>
                {
                    Assert.AreEqual("Hello Test World!", message.Message);
                    return Console.Out.WriteLineAsync(message.Message);
                });
            }
        }

        [Test]
        public void ReceiveMessage_StressTest()
        {
            using (var bus = new Bus())
            {
                var executionContext = new DataBus.TestExecutionContext(bus);
                DatabusExecutionContext.SetExecutionContext(executionContext);
                bus.Start();
                for (int i = 0; i < 1000; i++)
                {
                    executionContext.Publish<TestMessage>(new { Message = "Hello Test World!" }).Wait(message => Console.Out.WriteLineAsync(message.Message));
                }
            }
        }

    }

    public interface TestMessage
    {
        string Message { get; set; }
    }

    public class TestMessageHandler: MessageHandler<TestMessage>
    {
        public override Task MessageHandle(TestMessage message)
        {
            return Console.Out.WriteLineAsync(message.Message);
        }
    }
}
