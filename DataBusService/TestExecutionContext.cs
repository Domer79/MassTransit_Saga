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
using System.Threading;
using System.Threading.Tasks;
using DataBusService.Interfaces;
using MassTransit;
using MassTransit.Util;

namespace DataBusService
{
    public class TestExecutionContext: IExecutionContext, IDisposable
    {
        private readonly DataBus _dataBus;
        private readonly AutoResetEvent _publishResetEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _messageHandleResetEvent = new AutoResetEvent(false);
        private IDatabusSynchronizationContext _databusSynchronizationContext;

        public TestExecutionContext(DataBus dataBus)
        {
            _dataBus = dataBus;
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
            TaskUtil.Await(_dataBus.Publish(message));
            var databusSynchronizationContext = new DatabusSynchronizationContext<TMessage>(_publishResetEvent, _messageHandleResetEvent);
            _databusSynchronizationContext = databusSynchronizationContext;
            return databusSynchronizationContext;
        }

        public DatabusSynchronizationContext<TMessage> Publish<TMessage>(object message) where TMessage : class
        {
            TaskUtil.Await(_dataBus.Publish<TMessage>(message));
            var databusSynchronizationContext = new DatabusSynchronizationContext<TMessage>(_publishResetEvent, _messageHandleResetEvent);
            _databusSynchronizationContext = databusSynchronizationContext;
            return databusSynchronizationContext;
        }

        public IDatabusSynchronizationContext Publish(object message)
        {
            TaskUtil.Await(_dataBus.Publish(message));
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
            return _messageHandler((TMessage) message);
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
