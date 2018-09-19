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

using System.Linq;
using System.Reflection;
using DataBusService.Configuration;
using DataBusService.Interfaces;
using MassTransit;

namespace DataBusService
{
    public static class ConsumeExtensions
    {
        public static IMessageHandlerBuilder FromAssembly(this IBusFactoryConfigurator configurator, IHost host, Assembly assembly)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(BaseMessageHandler<>));

            return new MessageHandlerBuilder(handlerTypes.Select(_ =>
            {
                var messageType = _.BaseType.GetGenericArguments()[0];
                return new HandlerInfo()
                {
                    MessageHandlerType = _,
                    MessageType = messageType,
                    QueueGenerationType = QueueGenerationType.Default,
                    QueueName = messageType.FullName.Replace('.', '_')
                };
            }), configurator, host);
        }

        public static IMessageHandlerBuilder FromConfiguration(this IBusFactoryConfigurator configurator,
            IHost host)
        {
            var config = Config.GetRabbitMqConfigSection();
            var handlerInfos = config.MqHandlers
                .OfType<MqHandlerElement>()
                .Select(_ => new {Type = _.GetHandlerType(), _.Queue})
                .Where(t => t.Type.BaseType != null && t.Type.BaseType.IsGenericType && t.Type.BaseType.GetGenericTypeDefinition() == typeof(BaseMessageHandler<>))
                .Select(_ =>
                {
                    var messageType = _.Type.BaseType.GetGenericArguments()[0];
                    return new HandlerInfo()
                    {
                        MessageHandlerType = _.Type,
                        MessageType = messageType,
                        QueueGenerationType = QueueGenerationType.FromConfiguration,
                        QueueName = _.Queue ?? messageType.FullName.Replace('.', '_')
                    };
                });

            return new MessageHandlerBuilder(handlerInfos, configurator, host);
        }

        public static IMessageHandlerBuilder SetQueueByNameSpace(this IMessageHandlerBuilder builder)
        {
            var handlers = builder.GetHandlers().Select(_ => new HandlerInfo()
            {
                MessageType = _.MessageType,
                MessageHandlerType = _.MessageHandlerType,
                QueueGenerationType = _.QueueGenerationType,
                QueueName = _.MessageType.Namespace.Replace('.', '_')
            });

            return new MessageHandlerBuilder(handlers, builder.Configurator, builder.Host);
        }
    }
}