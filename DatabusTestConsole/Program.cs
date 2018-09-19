using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DataBusService;
using DataBusService.Interfaces;

namespace DatabusTestConsole
{
    class Program
    {
        public static int MessageCount = 10000;

        static void Main(string[] args)
        {
            DataBus bus = null;
            try
            {
                bus = new DataBus("domer");
                bus.Start();

                for (int i = 0; i < MessageCount; i++)
                {
                    bus.Publish<TestMessage>(new {Message = $"Hello World{i}"});
                }

                Thread.Sleep(TimeSpan.FromSeconds(20));
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

    public class TestMessageHandler : BaseMessageHandler<TestMessage>
    {
        public static int Counter;
        private static readonly Stopwatch Stopwatch = new Stopwatch();

        public override Task MessageHandle(TestMessage message)
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
    }
}
