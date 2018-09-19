using System;
using DataBusService;
using DataBusService.Configuration;
using MassTransit_Saga.CreateNewBook;
using NUnit.Framework;

namespace MassTransit_Saga.Tests
{
    [TestFixture]
    public class RabbitMqConfigurationTests
    {
        [Test]
        public void GetSection_IsNotNull_Test()
        {
            var section = Config.GetRabbitMqConfigSection();

            Assert.IsNotNull(section);
        }

        [Test]
        public void MqHandlers_Count_NotEmpty()
        {
            var section = Config.GetRabbitMqConfigSection();
            var handlers = section.MqHandlers;

            CollectionAssert.IsNotEmpty(handlers);
        }

        [Test]
        public void ElementMembers_IsValid_Test()
        {
            var section = Config.GetRabbitMqConfigSection();
            var element = section.MqHandlers[0];

            Type type = null;
            Assert.DoesNotThrow(() => type = element.GetHandlerType());
            Assert.IsNotNull(type);
            Assert.AreEqual(typeof(Message1Handler), Type.GetType(element.TypeName));
            Assert.AreEqual("message1_queue", element.Queue);
        }

        [TestCase("domer")]
        [TestCase("domer1")]
        public void ReadAttributesFromConnectionString_Ok_Test(string connectionName)
        {
            var section = Config.GetRabbitMqConfigSection();
            var connection = section.Connections[connectionName];
            var url = connection.Url;
            var userName = connection.UserName;
            var password = connection.Password;

            Assert.AreEqual("rabbitmq://domer-ss/", url);
            Assert.AreEqual("admin", userName);
            Assert.AreEqual("admin", password);
        }

        [TestCase("message1_queue")]
        public void ReadAttributesFromQueueElement_Ok_Test(string queueName)
        {
            var section = Config.GetRabbitMqConfigSection();
            var queueElement = section.Queues[queueName];

            var threadCount = queueElement.ThreadCount;
            var prefetchCountToCore = queueElement.PrefetchCountToThread;
            var threadsOfCore = queueElement.ThreadsByCoreCount;

            Assert.AreEqual(8, threadCount);
            Assert.AreEqual(1, prefetchCountToCore);
            Assert.AreEqual(false, threadsOfCore);
        }

        [TestCase("workMode")]
        public void ReadBusSettings_Ok_Test(string key)
        {
            var section = Config.GetRabbitMqConfigSection();
            var workMode = (WorkMode)Enum.Parse(typeof(WorkMode), section.BusSettings[key].Value, true);

            Assert.AreEqual(WorkMode.InMemory, workMode);
        }
    }
}
