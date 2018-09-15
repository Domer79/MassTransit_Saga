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
    public class TestExecutionContext: IExecutionContext, IDisposable
    {
        private readonly Bus _bus;
        private AutoResetEvent _publishResetEvent = new AutoResetEvent(false);
        private AutoResetEvent _messageHandleResetEvent = new AutoResetEvent(false);
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
                _messageHandleResetEvent.WaitOne();
                await _databusSynchronizationContext.MessageHandle(context.Message);
            }
            finally
            {
                _databusSynchronizationContext?.Dispose();
                _databusSynchronizationContext = null;
                _publishResetEvent.Set();
            }
        }

        public DatabusSynchronizationContext<TMessage> Publish<TMessage>(TMessage message) where TMessage : class
        {
            TaskUtil.Await(_bus.Publish(message));
            var databusSynchronizationContext = new DatabusSynchronizationContext<TMessage>(_publishResetEvent, _messageHandleResetEvent);
            _databusSynchronizationContext = databusSynchronizationContext;
            return databusSynchronizationContext;
        }

        public DatabusSynchronizationContext<TMessage> Publish<TMessage>(object message) where TMessage : class
        {
            TaskUtil.Await(_bus.Publish<TMessage>(message));
            var databusSynchronizationContext = new DatabusSynchronizationContext<TMessage>(_publishResetEvent, _messageHandleResetEvent);
            _databusSynchronizationContext = databusSynchronizationContext;
            return databusSynchronizationContext;
        }

        public IDatabusSynchronizationContext Publish(object message)
        {
            TaskUtil.Await(_bus.Publish(message));
            var databusSynchronizationContext = new DatabusSynchronizationContext<object>(_publishResetEvent, _messageHandleResetEvent);
            _databusSynchronizationContext = databusSynchronizationContext;
            return databusSynchronizationContext;
        }

        public void Dispose()
        {
            _publishResetEvent?.Dispose();
            _messageHandleResetEvent.Dispose();
        }
    }

    public interface IDatabusSynchronizationContext: IDisposable
    {
        Task MessageHandle(object message);
        void Wait(Func<object, Task> messageHandler);
    }

    public class DatabusSynchronizationContext<TMessage> : IDatabusSynchronizationContext where TMessage : class
    {
        private readonly AutoResetEvent _publishResetEvent;
        private readonly AutoResetEvent _messageHandleResetEvent;
        private Func<TMessage, Task> _messageHandler;

        public DatabusSynchronizationContext(AutoResetEvent publishResetEvent, AutoResetEvent messageHandleResetEvent)
        {
            _publishResetEvent = publishResetEvent;
            _messageHandleResetEvent = messageHandleResetEvent;
        }

        public Task MessageHandle(object message)
        {
            TaskUtil.Await(_messageHandler((TMessage) message));
            return Task.CompletedTask;
        }

        public void Wait(Func<TMessage, Task> messageHandler)
        {
            _messageHandler = messageHandler;
            _messageHandleResetEvent.Set();
            _publishResetEvent.WaitOne();
            Console.WriteLine("Message handled");
        }

        public void Wait(Func<object, Task> messageHandler)
        {
            Wait((Func<TMessage, Task>)messageHandler);
        }

        public void Dispose()
        {
            
        }
    }
}
