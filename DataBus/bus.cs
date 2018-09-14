using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBus.Configuration;
using MassTransit;

namespace DataBus
{
    public class Bus: IDisposable
    {
        private readonly string _url;
        private readonly string _userName;
        private readonly string _password;
        private IBusControl _busControl;
        private WorkMode _workMode;
        private readonly RabbitMqConfigSection _section;

        public Bus()
            :this(null)
        {

        }

        public Bus(string connectionName)
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

            Initialize();
        }

        private WorkMode GetWorkMode()
        {
            try
            {
                return (WorkMode)Enum.Parse(typeof(WorkMode), _section.BusSettings["workmode"].Value, true);
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
        }

        private IBusControl InitializeUsingRabbit()
        {
            return MassTransit.Bus.Factory.CreateUsingRabbitMq(x =>
            {
                var host = x.Host(new Uri(_url), c =>
                {
                    c.Username(_userName);
                    c.Password(_password);
                });

                x.FromConfiguration(host)
                    .Build();
            });
        }

        private IBusControl InitializeUsingInMemory()
        {
            return MassTransit.Bus.Factory.CreateUsingInMemory(x =>
            {
                x.FromConfiguration(x.Host)
                    .Build();
            });
        }

        public Task Publish(object message)
        {
            return _busControl.Publish(message);
        }

        public Task Publish<TMessage>(TMessage message) where TMessage : class
        {
            return _busControl.Publish<TMessage>(message);
        }

        public void Start()
        {
            _busControl.Start();
        }

        public Task StartAsync()
        {
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
            _busControl.Stop();
        }
    }

    public enum WorkMode
    {
        InMemory,
        RabbitMq
    }
}
