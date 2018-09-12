using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataBus;
using MassTransit;
using MassTransit_Saga.Contracts;
using MassTransit_Saga.CreateNewBook;
using NUnit.Framework;
using Tools;

namespace MassTransit_Saga.Tests
{
    [TestFixture]
    public class ConsumeBuilderTests
    {
        [Test]
        public void ZBuild_FromAssembly_Test()
        {
            var assemblyName = new AssemblyName("MassTransit_Saga.CreateNewBook");
            var assembly = Assembly.Load(assemblyName);
            var bc = Bus.Factory.CreateUsingInMemory(x =>
            {
                var messageHandlerConfigurator = x.FromAssembly(x.Host, assembly);
                var handlerInfos = messageHandlerConfigurator.GetHandlers();
                messageHandlerConfigurator.Build();

                Assert.IsTrue(handlerInfos.Any());
            });
        }

        [Test]
        public void ZBuild_FromConfiguration_Test()
        {
            var bc = Bus.Factory.CreateUsingInMemory(x =>
            {
                var messageHandlerConfigurator = x.FromConfiguration(x.Host);
                var handlerInfos = messageHandlerConfigurator.GetHandlers();
                messageHandlerConfigurator.Build();

                Assert.IsTrue(handlerInfos.Any());
            });
        }

        [Test]
        public void Message1Handler_BaseType_is_MessageHandler()
        {
            var type = typeof(Message1Handler);

            Assert.IsNotNull(type.BaseType);
            Assert.AreEqual(type.BaseType.GetGenericTypeDefinition(), typeof(DataBus.Interfaces.MessageHandler<>));
        }

        [Test]
        public void Message1Handler_Find_Ok_Test()
        {
            var assemblyName = new AssemblyName("MassTransit_Saga.CreateNewBook");
            var assembly = Assembly.Load(assemblyName);
            var bc = Bus.Factory.CreateUsingInMemory(x =>
            {
                var messageHandlerConfigurator = x.FromAssembly(x.Host, assembly);
                var messageHandlersDictionary = ((MessageHandlerConfigurator)messageHandlerConfigurator).MessageHandlersDictionary;
                messageHandlerConfigurator.Build();

                Assert.AreEqual(typeof(Message1Handler), messageHandlersDictionary[typeof(Message1)].MessageHandlerType);
            });
        }

        [Test]
        public void Message1Handler_QueueName_Expected_ByNameSpace_Test()
        {
            var assemblyName = new AssemblyName("MassTransit_Saga.CreateNewBook");
            var assembly = Assembly.Load(assemblyName);
            var bc = Bus.Factory.CreateUsingInMemory(x =>
            {
                var messageHandlerConfigurator = x.FromAssembly(x.Host, assembly).SetQueueByNameSpace();
                var messageHandlersDictionary = ((MessageHandlerConfigurator)messageHandlerConfigurator).MessageHandlersDictionary;
                messageHandlerConfigurator.Build();

                Assert.AreEqual("MassTransit_Saga_Contracts", messageHandlersDictionary[typeof(Message1)].QueueName);
            });
        }

        [TestCase("MassTransit_Saga.Contracts.BookStatus, MassTransit_Saga.Contracts")]
        public void TypeGetType_IsNotNull(string typeName)
        {
            var type = Type.GetType(typeName);

            Assert.IsNotNull(type);
        }
    }
}
