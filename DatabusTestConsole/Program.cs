using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using DataBusService;
using DataBusService.Interfaces;
using MassTransit;
using MassTransit.Context;
using Ninject;

namespace DatabusTestConsole
{
    class Program
    {
        public static int MessageCount = 10;

        static void Main(string[] args)
        {
            DataBus bus = null;
            DependencyResolver.SetDependencyResolver(new NinjectBusDependencyResolver());
            try
            {
                EndpointConvention.Map<TestMessage>(new Uri("rabbitmq://localhost/testmessage_queue"));
                MessageCorrelation.UseCorrelationId<TestMessage>(x => x.CorrelationId);
                bus = new DataBus("domer");
                bus.Start();

                for (int i = 0; i < MessageCount; i++)
                {
                    bus.Publish<TestMessage>(new TestMessage(){Message = $"Hello World{i}"}).Wait();
                }

                //Thread.Sleep(TimeSpan.FromSeconds(20));
                Console.WriteLine($"Количество обработанных сообщений: {TestMessageHandler.Counter}");
                Console.WriteLine($"Время затрачено: {TestMessageHandler.WorkTime}");
                Console.ReadLine();

            }
            finally
            {
                bus?.Stop();
            }
        }
    }

    public class AutofacBusDependencyResolver: IBusDependencyResolver
    {
        private readonly IContainer _container;

        public AutofacBusDependencyResolver()
        {
            _container = IocConfigure();
        }

        public TService Resolve<TService>()
        {
            return _container.Resolve<TService>();
        }

        public object Resolve(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }

        /// <summary>
        /// Resolve handler
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="messageHandlerType"></param>
        /// <returns></returns>
        public BaseMessageHandler<TMessage> ResolveHandler<TMessage>(Type messageHandlerType) where TMessage : class
        {
            return (BaseMessageHandler<TMessage>) _container.Resolve(messageHandlerType);
        }

        private static IContainer IocConfigure()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Interface1Implement>().As<IInterface1>();
            builder.RegisterType<Interface2Implement>().As<IInterface2>();

            return builder.Build();
        }
    }

    public class NinjectBusDependencyResolver: IBusDependencyResolver
    {
        private readonly IKernel _kernel;

        public NinjectBusDependencyResolver()
        {
            _kernel = IocConfigure();
        }

        public TService Resolve<TService>()
        {
            return _kernel.Get<TService>();
        }

        public object Resolve(Type serviceType)
        {
            return _kernel.Get(serviceType);
        }

        /// <summary>
        /// Resolve handler
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="messageHandlerType"></param>
        /// <returns></returns>
        public BaseMessageHandler<TMessage> ResolveHandler<TMessage>(Type messageHandlerType) where TMessage : class
        {
            return (BaseMessageHandler<TMessage>) _kernel.Get(messageHandlerType);
        }

        private static IKernel IocConfigure()
        {
            var kernel = new StandardKernel();

            kernel.Bind<IInterface1>().To<Interface1Implement>().InThreadScope();
            kernel.Bind<IInterface2>().To<Interface2Implement>().InThreadScope();

            return kernel;
        }
    }

    public class TestMessageHandler : BaseMessageHandler<TestMessage>
    {
        public static int Counter;
        private static readonly Stopwatch Stopwatch = new Stopwatch();

        public override Task MessageHandle(TestMessage message, ConsumeContext<TestMessage> context)
        {
            Counter++;

            if (Counter == 1)
            {
                Console.WriteLine("Поехали!");
                Stopwatch.Start();
            }

            if (Counter == Program.MessageCount)
                WorkTime = Stopwatch.Elapsed;
            return Task.CompletedTask;
//            return Console.Out.WriteLineAsync(message.Message);
        }

        public static TimeSpan WorkTime { get; set; }
    }

    public class TestMessage
    {
        public string Message { get; set; }
        public Guid CorrelationId { get; set; }
    }

    public class Test2MessageHandler : BaseMessageHandler<TestMessage>
    {
        private readonly IInterface1 _implement1;
        private readonly IInterface2 _implement2;

        public Test2MessageHandler(IInterface1 implement1, IInterface2 implement2)
        {
            _implement1 = implement1;
            _implement2 = implement2;
        }

        public override async Task MessageHandle(TestMessage message, ConsumeContext<TestMessage> context)
        {
            await Console.Out.WriteLineAsync(message.Message);
            await Console.Out.WriteLineAsync(_implement1.InstanceName);
            await Console.Out.WriteLineAsync(_implement2.InstanceName);
        }
    }

    public interface IInterface1
    {
        string InstanceName { get; }
    }

    public interface IInterface2
    {
        string InstanceName { get; }
    }

    public class Interface1Implement : IInterface1
    {
        public Interface1Implement()
        {
            Console.WriteLine("Interface1Implement Created");
        }

        public string InstanceName => "Instance1";
    }

    public class Interface2Implement : IInterface2
    {
        public Interface2Implement()
        {
            Console.WriteLine("Interface2Implement Created");
        }

        public string InstanceName => "Instance2";
    }
}
