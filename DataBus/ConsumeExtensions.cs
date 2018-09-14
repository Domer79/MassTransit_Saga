using System.Linq;
using System.Reflection;
using DataBus.Configuration;
using DataBus.Interfaces;
using MassTransit;

namespace DataBus
{
    public static class ConsumeExtensions
    {
        public static IMessageHandlerBuilder FromAssembly(this IBusFactoryConfigurator configurator, IHost host, Assembly assembly)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(Interfaces.MessageHandler<>));

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
                .Where(t => t.Type.BaseType != null && t.Type.BaseType.IsGenericType && t.Type.BaseType.GetGenericTypeDefinition() == typeof(Interfaces.MessageHandler<>))
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