using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DataBus.Interfaces;
using MassTransit;
using MassTransit.RabbitMqTransport;

namespace DataBus
{
    public class MessageHandlerConfigurator: IMessageHandlerConfigurator
    {
        private readonly IEnumerable<HandlerInfo> _handlerInfos;
        private readonly IBusFactoryConfigurator _configurator;
        private readonly IHost _host;
        internal Dictionary<Type, HandlerInfo> MessageHandlersDictionary { get; }

        public MessageHandlerConfigurator(IEnumerable<HandlerInfo> handlerInfos, IBusFactoryConfigurator configurator, IHost host)
        {
            _handlerInfos = handlerInfos;
            _configurator = configurator;
            _host = host;
            MessageHandlersDictionary = handlerInfos.ToDictionary(_ => _.MessageType);
        }

        public IBusFactoryConfigurator Configurator => _configurator;
        public IHost Host => _host;

        public void Build()
        {
            if (_configurator is IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator)
            {
                RabbitMqBuild(rabbitMqBusFactoryConfigurator);
                return;
            }

            if (_configurator is IBusFactoryConfigurator busFactoryConfigurator)
                DefaultBuild(busFactoryConfigurator);
        }

        private void DefaultBuild(IBusFactoryConfigurator configurator)
        {
            var groupHandlers = _handlerInfos.GroupBy(_ => _.QueueName);
            foreach (var groupHandler in groupHandlers)
            {
                configurator.ReceiveEndpoint(groupHandler.Key, cfg =>
                {
                    foreach (var handlerInfo in groupHandler)
                    {
                        var handlerMethod = GetHandlerMethod(handlerInfo.MessageType);
                        var methodInfo = GetType().GetMethod("Handler").MakeGenericMethod(handlerInfo.MessageType);
                        var delgate = Delegate.CreateDelegate(typeof(MassTransit.MessageHandler<>).MakeGenericType(handlerInfo.MessageType), this, methodInfo);

                        handlerMethod.Invoke(null, new object[] { cfg, delgate, null });
                    }
                });
            }
        }

        private void RabbitMqBuild(IRabbitMqBusFactoryConfigurator configurator)
        {
            var groupHandlers = _handlerInfos.GroupBy(_ => _.QueueName);
            foreach (var groupHandler in groupHandlers)
            {
                configurator.ReceiveEndpoint((IRabbitMqHost)_host, groupHandler.Key, cfg =>
                {
                    foreach (var handlerInfo in groupHandler)
                    {
                        var handlerMethod = GetHandlerMethod(handlerInfo.MessageType);
                        var methodInfo = GetType().GetMethod("Handler").MakeGenericMethod(handlerInfo.MessageType);
                        var delgate = Delegate.CreateDelegate(typeof(MassTransit.MessageHandler<>).MakeGenericType(handlerInfo.MessageType), this, methodInfo);

                        handlerMethod.Invoke(null, new object[] { cfg, delgate, null });
                    }
                });
            }
        }

        private static MethodInfo GetHandlerMethod(Type handlerType)
        {
            var methodInfo = typeof(HandlerExtensions).GetMethod("Handler");
            return methodInfo.MakeGenericMethod(handlerType);
        }

        public Task Handler<TMessage>(ConsumeContext<TMessage> context) where TMessage : class
        {
            var messageHandler = (Interfaces.MessageHandler<TMessage>)Activator.CreateInstance(MessageHandlersDictionary[typeof(TMessage)].MessageHandlerType);
            return messageHandler.MessageHandle(context.Message);
        }

        public IEnumerable<HandlerInfo> GetHandlers()
        {
            return _handlerInfos;
        }
    }
}