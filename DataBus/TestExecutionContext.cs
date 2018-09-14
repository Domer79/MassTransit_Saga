using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataBus.Interfaces;
using MassTransit;
using MassTransit.Util;

namespace DataBus
{
    public class TestExecutionContext: IExecutionContext
    {
        private readonly Bus _bus;
        private IDatabusSynchronizationContext _databusSynchronizationContext;

        public TestExecutionContext(Bus bus)
        {
            _bus = bus;
        }

        public void SetHandlers(Dictionary<Type, HandlerInfo> messageHandlersDictionary)
        {
            throw new NotSupportedException();
        }

        public async Task Handler<TMessage>(ConsumeContext<TMessage> context) where TMessage : class
        {
            try
            {
                await _databusSynchronizationContext.MessageHandle(context.Message);
            }
            finally
            {
                _databusSynchronizationContext.Dispose();
            }
        }

        public DatabusSynchronizationContext<TMessage> Publish<TMessage>(TMessage message) where TMessage : class
        {
            TaskUtil.Await(_bus.Publish(message));
            var databusSynchronizationContext = new DatabusSynchronizationContext<TMessage>();
            _databusSynchronizationContext = databusSynchronizationContext;
            return databusSynchronizationContext;
        }

        public DatabusSynchronizationContext<TMessage> Publish<TMessage>(object message) where TMessage : class
        {
            TaskUtil.Await(_bus.Publish<TMessage>(message));
            var databusSynchronizationContext = new DatabusSynchronizationContext<TMessage>();
            _databusSynchronizationContext = databusSynchronizationContext;
            return databusSynchronizationContext;
        }

        public IDatabusSynchronizationContext Publish(object message)
        {
            TaskUtil.Await(_bus.Publish(message));
            var databusSynchronizationContext = new DatabusSynchronizationContext<object>();
            _databusSynchronizationContext = databusSynchronizationContext;
            return databusSynchronizationContext;
        }
    }

    public interface IDatabusSynchronizationContext: IDisposable
    {
        Task MessageHandle(object message);
        void Wait(Func<object, Task> messageHandler);
    }

    public class DatabusSynchronizationContext<TMessage> : IDatabusSynchronizationContext where TMessage : class
    {
        private readonly AutoResetEvent _autoEvent = new AutoResetEvent(false);
        private Func<TMessage, Task> _messageHandler;

        public Task MessageHandle(object message)
        {
            TaskUtil.Await(_messageHandler((TMessage) message));
            _autoEvent.Set();
            return Task.CompletedTask;
        }

        public void Wait(Func<TMessage, Task> messageHandler)
        {
            _messageHandler = messageHandler;
            _autoEvent.WaitOne();
            Console.WriteLine("Message handled");
        }

        public void Wait(Func<object, Task> messageHandler)
        {
            Wait((Func<TMessage, Task>)messageHandler);
        }

        public void Dispose()
        {
            _autoEvent?.Dispose();
        }
    }
}
