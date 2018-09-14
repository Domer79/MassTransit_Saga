using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DataBus.Configuration;
using DataBus.Interfaces;
using GreenPipes;
using MassTransit;
using MassTransit.RabbitMqTransport;

namespace DataBus
{
    public class MessageHandlerBuilder: IMessageHandlerBuilder
    {
        private readonly IEnumerable<HandlerInfo> _handlerInfos;
        private readonly IBusFactoryConfigurator _configurator;
        private readonly IHost _host;
        internal Dictionary<Type, HandlerInfo> MessageHandlersDictionary { get; }

        public MessageHandlerBuilder(IEnumerable<HandlerInfo> handlerInfos, IBusFactoryConfigurator configurator, IHost host)
        {
            _handlerInfos = handlerInfos;
            _configurator = configurator;
            _host = host;
            MessageHandlersDictionary = handlerInfos.ToDictionary(_ => _.MessageType);

            if (DatabusExecutionContext.Current == null)
                throw new InvalidOperationException("Execution context is missing.");

            try
            {
                DatabusExecutionContext.Current.SetHandlers(MessageHandlersDictionary);
            }
            catch (NotSupportedException)
            {
            }
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
                InMemoryBuild(busFactoryConfigurator);
        }

        private void InMemoryBuild(IBusFactoryConfigurator configurator)
        {
            var groupHandlers = _handlerInfos.GroupBy(_ => _.QueueName);
            foreach (var groupHandler in groupHandlers)
            {
                configurator.ReceiveEndpoint(groupHandler.Key, cfg =>
                {
                    foreach (var handlerInfo in groupHandler)
                    {
                        var handlerMethod = GetHandlerMethod(handlerInfo.MessageType);
                        var methodInfo = DatabusExecutionContext.Current.GetType().GetMethod("Handler").MakeGenericMethod(handlerInfo.MessageType);
                        var delgate = Delegate.CreateDelegate(typeof(MassTransit.MessageHandler<>).MakeGenericType(handlerInfo.MessageType), DatabusExecutionContext.Current, methodInfo);

                        handlerMethod.Invoke(null, new object[] { cfg, delgate, null });
                    }

                    EndpointConfigure(cfg, groupHandler.Key);
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
                        var methodInfo = DatabusExecutionContext.Current.GetType().GetMethod("Handler").MakeGenericMethod(handlerInfo.MessageType);
                        var delgate = Delegate.CreateDelegate(typeof(MassTransit.MessageHandler<>).MakeGenericType(handlerInfo.MessageType), DatabusExecutionContext.Current, methodInfo);

                        handlerMethod.Invoke(null, new object[] { cfg, delgate, null });
                    }

                    EndpointConfigure(cfg, groupHandler.Key);
                });
            }
        }

        private void EndpointConfigure(IReceiveEndpointConfigurator endpointConfigurator, string queue)
        {
            var endpointConfig = Config.GetRabbitMqConfigSection().Queues[queue];
            if (endpointConfig == null)
                return;

            var threadCount = endpointConfig.ThreadsByCoreCount
                ? Environment.ProcessorCount
                : endpointConfig.ThreadCount;

            var prefetchCount = endpointConfig.PrefetchCountToThread * threadCount;

            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rabbitConfigurator)
                rabbitConfigurator.PrefetchCount = (ushort)prefetchCount;

            endpointConfigurator.UseConcurrencyLimit(threadCount);
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