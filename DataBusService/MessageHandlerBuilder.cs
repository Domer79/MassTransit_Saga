//Copyright 2018 Damir Garipov
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DataBusService.Configuration;
using DataBusService.Interfaces;
using GreenPipes;
using MassTransit;
using MassTransit.RabbitMqTransport;

namespace DataBusService
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
                        var delgate = Delegate.CreateDelegate(typeof(MessageHandler<>).MakeGenericType(handlerInfo.MessageType), DatabusExecutionContext.Current, methodInfo);

                        handlerMethod.Invoke(null, new object[] { cfg, delgate, null });
                        Console.WriteLine($"{handlerInfo.MessageHandlerType.Name} for message {handlerInfo.MessageType.Name} in queue {handlerInfo.QueueName} registered");
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
                        var delgate = Delegate.CreateDelegate(typeof(MessageHandler<>).MakeGenericType(handlerInfo.MessageType), DatabusExecutionContext.Current, methodInfo);

                        handlerMethod.Invoke(null, new object[] { cfg, delgate, null });
                        Console.WriteLine($"{handlerInfo.MessageHandlerType.Name} for message {handlerInfo.MessageType.Name} in queue {handlerInfo.QueueName} registered");
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

            var prefetchCount = (ushort) (endpointConfig.PrefetchCountToThread * threadCount);

            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rabbitConfigurator)
                rabbitConfigurator.PrefetchCount = prefetchCount;

            endpointConfigurator.UseConcurrencyLimit(threadCount);
        }

        private static MethodInfo GetHandlerMethod(Type handlerType)
        {
            var methodInfo = typeof(HandlerExtensions).GetMethod("Handler");
            return methodInfo.MakeGenericMethod(handlerType);
        }

        public Task Handler<TMessage>(ConsumeContext<TMessage> context) where TMessage : class
        {
            var messageHandler = (BaseMessageHandler<TMessage>)Activator.CreateInstance(MessageHandlersDictionary[typeof(TMessage)].MessageHandlerType);
            return messageHandler.MessageHandle(context.Message);
        }

        public IEnumerable<HandlerInfo> GetHandlers()
        {
            return _handlerInfos;
        }
    }
}