using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit_Saga.CreateNewBook;
using NUnit.Framework;
using RabbitMqConfiguration;

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

        [Test]
        public void ReadAttributesFromConfigSection_Ok_Test()
        {
            var section = Config.GetRabbitMqConfigSection();
            var host = section.Host;
            var userName = section.UserName;
            var password = section.Password;

            Assert.AreEqual("rabbitmq://domer-ss/", host);
            Assert.AreEqual("admin", userName);
            Assert.AreEqual("admin", password);
        }
    }
}
