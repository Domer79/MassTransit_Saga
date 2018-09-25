using System;
using DataBusService;
using MassTransit_Saga.Tests.Contracts;
using NUnit.Framework;
using TestExecutionContext = DataBusService.TestExecutionContext;

namespace MassTransit_Saga.Tests
{
    [TestFixture()]
    public class DataBusTests
    {
        [Test]
        public void ReceiveMessage_Test()
        {
            using (var bus = new DataBus())
            {
                using (var executionContext = new TestExecutionContext(bus))
                {
                    DatabusExecutionContext.SetExecutionContext(executionContext);
                    bus.Start();
                    executionContext.Publish<TestMessage>(new {Message = "Hello Test World!"}).Wait(message =>
                    {
                        Assert.AreEqual("Hello Test World!", message.Message);
                        return Console.Out.WriteLineAsync(message.Message);
                    });
                }
            }
        }

        [Test]
        public void ReceiveMessage_StressTest()
        {
            using (var bus = new DataBus())
            {
                using (var executionContext = new TestExecutionContext(bus))
                {
                    DatabusExecutionContext.SetExecutionContext(executionContext);
                    bus.Start();
                    for (int i = 0; i < 1000; i++)
                    {
                        executionContext.Publish<TestMessage>(new {Message = "Hello Test World!"})
                            .Wait(message => Console.Out.WriteLineAsync(message.Message));
                    }
                }
            }
        }

    }
}
