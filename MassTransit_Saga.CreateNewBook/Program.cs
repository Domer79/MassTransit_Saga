﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Castle.DynamicProxy.Contributors;
using DataBus;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.RabbitMqTransport;
using MassTransit.RabbitMqTransport.Configuration;
using MassTransit_Saga.Contracts;
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

    public enum Command
    {
        Exit = -1,
        None = 0,
        CreateBook = 1
    }

    public class Message1Handler : DataBus.Interfaces.MessageHandler<Message1>
    {
        public override Task MessageHandle(Message1 message)
        {
            return Console.Out.WriteLineAsync(message.Message);
        }
    }

    public class Message2Handler : DataBus.Interfaces.MessageHandler<Message2>
    {
        public override Task MessageHandle(Message2 message)
        {
            return Console.Out.WriteLineAsync(message.Message);
        }
    }
}
