using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Castle.DynamicProxy.Contributors;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.RabbitMqTransport;
using MassTransit.RabbitMqTransport.Configuration;
using MassTransit_Saga.Contracts;
using RabbitMqConfiguration;
using Tools;

namespace MassTransit_Saga.CreateNewBook
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterConsumers(Assembly.GetExecutingAssembly());
            builder.Register(context =>
            {
                var bc = Bus.Factory.CreateUsingRabbitMq(x =>
                {
                    var host = x.Host(new Uri("rabbitmq://domer-ss/"), h =>
                    {
                        h.Username("admin");
                        h.Password("admin");
                    });

//                    x.FromAssembly(host, Assembly.GetExecutingAssembly()).SetQueueByNameSpace().Build();
                    x.FromConfiguration(host).SetQueueByNameSpace().Build();
                });

                return bc;
            })
                .SingleInstance()
                .As<IBusControl>()
                .As<IBus>();

            var container = builder.Build();

            var busControl = container.Resolve<IBusControl>();
            try
            {

                busControl.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            Command command;
            do
            {
                Console.Write("Write command: ");

                try
                {
                    command = (Command)Enum.Parse(typeof(Command), Console.ReadLine());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    command = Command.None;
                }

                switch (command)
                {
                    case Command.Exit:
                    {
                        break;
                    }
                    case Command.CreateBook:
                    {
                        busControl.Publish<IBookCreated>(new BookCreated()
                        {
                            CorrelationId = Guid.NewGuid(),
                            AdditionalRequirements = "Хочу блинчики",
                            ArrivalTime = DateTimeOffset.Now,
                            DepartureTime = DateTimeOffset.Now.AddDays(1),
                            ContactFirstName = "Damir",
                            ContactLastName = "Garipov",
                            ContactEmail = "garipovd@mail.ru",
                            ContactPhone = "1234567",
                            Created = DateTimeOffset.Now,
                            ObjectRoomIds = new[]{1,2,3},
                            RatePlanId = 1,
                        });
                        break;
                    }

                    default:
                    {
                        command = Command.None;
                        continue;
                    }
                }
                
            } while (command != Command.Exit);
        }
    }

    internal class Message1Consumer: IConsumer<Message1>
    {
        public Task Consume(ConsumeContext<Message1> context)
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            return Console.Out.WriteLineAsync(context.Message.Message);
        }
    }

    public enum Command
    {
        Exit = -1,
        None = 0,
        CreateBook = 1
    }

    public abstract class MessageHandler<TMessage>
        where TMessage : class
    {
        public abstract Task MessageHandle(TMessage message);
    }

    public class Message1Handler : MessageHandler<Message1>
    {
        public override Task MessageHandle(Message1 message)
        {
            return Console.Out.WriteLineAsync(message.Message);
        }
    }

    public class Message2Handler : MessageHandler<Message2>
    {
        public override Task MessageHandle(Message2 message)
        {
            return Console.Out.WriteLineAsync(message.Message);
        }
    }

    public static class ConsumeExtensions
    {
        public static IMessageHandlerConfigurator FromAssembly(this IBusFactoryConfigurator configurator, IHost host, Assembly assembly)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(MessageHandler<>));

            return new MessageHandlerConfigurator(handlerTypes.Select(_ =>
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

        public static IMessageHandlerConfigurator FromConfiguration(this IBusFactoryConfigurator configurator,
            IHost host)
        {
            var config = Config.GetRabbitMqConfigSection();
            var handlerInfos = config.MqHandlers
                .OfType<MqHandlerElement>()
                .Select(_ => new {Type = _.GetHandlerType(), _.Queue})
                .Where(t => t.Type.BaseType != null && t.Type.BaseType.IsGenericType && t.Type.BaseType.GetGenericTypeDefinition() == typeof(MessageHandler<>))
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

            return new MessageHandlerConfigurator(handlerInfos, configurator, host);
        }

        public static IMessageHandlerConfigurator SetQueueByNameSpace(this IMessageHandlerConfigurator configurator)
        {
            var handlers = configurator.GetHandlers().Select(_ => new HandlerInfo()
            {
                MessageType = _.MessageType,
                MessageHandlerType = _.MessageHandlerType,
                QueueGenerationType = _.QueueGenerationType,
                QueueName = _.MessageType.Namespace.Replace('.', '_')
            });

            return new MessageHandlerConfigurator(handlers, configurator.Configurator, configurator.Host);
        }
    }

    public class HandlerInfo
    {
        public Type MessageHandlerType { get; set; }
        public Type MessageType { get; set; }
        public QueueGenerationType QueueGenerationType { get; set; }
        public string QueueName { get; set; }
    }

    public enum QueueGenerationType
    {
        Default,
        ByNameSpace,
        FromConfiguration
    }

    public interface IMessageHandlerConfigurator
    {
        IBusFactoryConfigurator Configurator { get; }
        IHost Host { get; }
        void Build();
        IEnumerable<HandlerInfo> GetHandlers();
    }

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
            var messageHandler = (MessageHandler<TMessage>)Activator.CreateInstance(MessageHandlersDictionary[typeof(TMessage)].MessageHandlerType);
            return messageHandler.MessageHandle(context.Message);
        }

        public IEnumerable<HandlerInfo> GetHandlers()
        {
            return _handlerInfos;
        }
    }

    public class ConsumeBuilder
    {
        private readonly IHost _host;
        private readonly IBusFactoryConfigurator _configurator;

        public ConsumeBuilder(IHost host, IBusFactoryConfigurator configurator)
        {
            _host = host;
            _configurator = configurator;
        }

        public void Build(Assembly assembly)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(MessageHandler<>));

            Types = handlerTypes
                .ToDictionary(t => t.BaseType.GetGenericArguments()[0]);

            if (_configurator is IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator)
            {
                RabbitMqBuild(rabbitMqBusFactoryConfigurator);
                return;
            }

            if (_configurator is IBusFactoryConfigurator busFactoryConfigurator)
                DefaultBuild(busFactoryConfigurator);
        }

        internal Dictionary<Type, Type> Types { get; private set; }

        private void DefaultBuild(IBusFactoryConfigurator configurator)
        {
            foreach (var type in Types)
            {
                var handlerMethod = GetHandlerMethod(type.Key);
                var methodInfo = GetType().GetMethod("Handler").MakeGenericMethod(type.Key);
                var delgate = Delegate.CreateDelegate(typeof(MassTransit.MessageHandler<>).MakeGenericType(type.Key), this,
                    methodInfo);

                configurator.ReceiveEndpoint(type.Key.FullName.Replace('.', '_'), cfg =>
                {
                    handlerMethod.Invoke(null, new object[] {cfg, delgate, null});
                });
            }
        }

        private void RabbitMqBuild(IRabbitMqBusFactoryConfigurator configurator)
        {
            foreach (var type in Types)
            {
                var handlerMethod = GetHandlerMethod(type.Key);
                var methodInfo = GetType().GetMethod("Handler").MakeGenericMethod(type.Key);
                var delgate = Delegate.CreateDelegate(typeof(MassTransit.MessageHandler<>).MakeGenericType(type.Key), this,
                    methodInfo);
                configurator.ReceiveEndpoint((IRabbitMqHost)_host, type.Key.FullName.Replace('.', '_'), cfg =>
                {
                    handlerMethod.Invoke(null, new object[] {cfg, delgate, null});
                });
            }
        }

        private static MethodInfo GetHandlerMethod(Type handlerType)
        {
            var methodInfo = typeof(HandlerExtensions).GetMethod("Handler");
            return methodInfo.MakeGenericMethod(handlerType);
        }

        public Task Handler<TMessage>(ConsumeContext<TMessage> context) where TMessage: class
        {
            var messageHandler = (MessageHandler<TMessage>)Activator.CreateInstance(Types[typeof(TMessage)]);
            return messageHandler.MessageHandle(context.Message);
        }
    }
}
