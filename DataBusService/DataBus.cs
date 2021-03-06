﻿//Copyright 2018 Damir Garipov
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
using System.Diagnostics;
using System.Threading.Tasks;
using DataBusService.Configuration;
using DataBusService.Interfaces;
using GreenPipes;
using MassTransit;
using MassTransit.NLogIntegration;

namespace DataBusService
{
    public class DataBus: IDisposable, IPublisher
    {
        private readonly string _url;
        private readonly string _userName;
        private readonly string _password;
        private IBusControl _busControl;
        private readonly WorkMode _workMode;
        private readonly RabbitMqConfigSection _section;
        private ConnectHandle _receiveHandle;
        private ConnectHandle _consumeHandle;

        public DataBus()
            :this(null)
        {

        }

        public DataBus(string connectionName)
        {
            _section = Config.GetRabbitMqConfigSection();
            _workMode = GetWorkMode();

            try
            {
                var connection = connectionName == null
                    ? _section.Connections[0]
                    : _section.Connections[connectionName];

                if (string.IsNullOrWhiteSpace(connection.Url))
                    throw new ArgumentException("connection.Url");

                _url = connection.Url;
                _userName = connection.UserName;
                _password = connection.Password;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                _workMode = WorkMode.InMemory;
            }
        }

        public Task Publish(object message)
        {
            return _busControl.Publish(message);
        }

        public Task Publish<TMessage>(TMessage message) where TMessage : class
        {
            return _busControl.Publish<TMessage>(message);
        }

        public Task Publish<TMessage>(object message) where TMessage : class
        {
            return _busControl.Publish<TMessage>(message);
        }

        public Task Send<TMessage>(TMessage message) where TMessage : class
        {
            return _busControl.Send<TMessage>(message);
        }

        public async Task Send<TMessage>(object message) where TMessage : class
        {
            await _busControl.Send<TMessage>(message);
        }

        public void Start()
        {
            Initialize();
            _busControl.Start();
        }

        public Task StartAsync()
        {
            Initialize();
            return _busControl.StartAsync();
        }

        public void Stop()
        {
            _busControl.Stop();
        }

        public Task StopAsync()
        {
            return _busControl.StopAsync();
        }

        public void Dispose()
        {
            Log.Info("Start Dispose DataBus");
            _receiveHandle.Disconnect();
            _receiveHandle.Dispose();
            _consumeHandle.Disconnect();
            _consumeHandle.Dispose();
            _busControl.Stop();
            Log.Info("DataBus Disposed");
        }

        private WorkMode GetWorkMode()
        {
            try
            {
                return (WorkMode)Enum.Parse(typeof(WorkMode), _section.BusSettings["workMode"].Value, true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return WorkMode.RabbitMq;
            }
        }

        private void Initialize()
        {
            _busControl = _workMode == WorkMode.RabbitMq
                ? InitializeUsingRabbit()
                : InitializeUsingInMemory();

            var receiveObserver = new ReceiveObserver();
            var consumeObserver = new ConsumeObserver();
            _receiveHandle = _busControl.ConnectReceiveObserver(receiveObserver);
            _consumeHandle = _busControl.ConnectConsumeObserver(consumeObserver);
        }

        private IBusControl InitializeUsingRabbit()
        {
            return Bus.Factory.CreateUsingRabbitMq(x =>
            {
                var host = x.Host(new Uri(_url), c =>
                {
                    c.Username(_userName);
                    c.Password(_password);
                });

                x.FromConfiguration(host)
                    .Build();

                x.UseNLog();
            });
        }

        private IBusControl InitializeUsingInMemory()
        {
            return Bus.Factory.CreateUsingInMemory(x =>
            {
                x.FromConfiguration(x.Host)
                    .Build();
            });
        }
    }
}
