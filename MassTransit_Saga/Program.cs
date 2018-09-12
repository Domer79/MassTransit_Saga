using System;
using System.Configuration;
using System.Data;
using Automatonymous;
using AutoMapper;
using MassTransit;
using MassTransit.EntityFrameworkIntegration;
using MassTransit.EntityFrameworkIntegration.Saga;
using MassTransit_Saga.Contracts;

namespace MassTransit_Saga
{
    class Program
    {
        static void Main(string[] args)
        {
            MapperMappings.Map();

            var sagaStateMachine = new BookStateMachine();
            string connectionString = ConfigurationManager.ConnectionStrings["sagabook"].ConnectionString;
            SagaDbContextFactory contextFactory = () =>
                new SagaDbContext<Book, SagaInstanceMap>(connectionString);
            var repository = new EntityFrameworkSagaRepository<Book>(contextFactory, optimistic:true);

            var busControl = Bus.Factory.CreateUsingRabbitMq(x =>
            {
                var host = x.Host(new Uri("rabbitmq://domer-ss/"), h =>
                {
                    h.Username("admin");
                    h.Password("admin");
                });

                x.ReceiveEndpoint(host, "book_guest", e =>
                {
                    e.StateMachineSaga(sagaStateMachine, repository);
                });
            });

            busControl.Start();
            ConsoleKey consoleKey = ConsoleKey.NoName;
            do
            {
                if (consoleKey == ConsoleKey.F1)
                    busControl.Publish<Message1>(new {Message = "Hello World 1!"});

                if (consoleKey == ConsoleKey.F2)
                    busControl.Publish<Message2>(new {Message = "Hello World 2!"});
            } while ((consoleKey = Console.ReadKey().Key) != ConsoleKey.Escape);

            busControl.Stop();
        }
    }

    public class Book : SagaStateMachineInstance
    {
        public string CurrentState { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid? ExpirationId { get; set; }
        public byte[] RowVersion { get; set; }

        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public DateTimeOffset ArrivalTime { get; set; }
        public DateTimeOffset DepartureTime { get; set; }
        public int RatePlanId { get; set; }
        public int[] ObjectRoomIds { get; set; }
        public string AdditionalRequirements { get; set; }
        public BookStatus Status { get; set; }
        public DateTimeOffset Created { get; set; }
    }

    public class BookStateMachine : MassTransitStateMachine<Book>
    {
        public BookStateMachine()
        {
            InstanceState(x => x.CurrentState);
            Event(() => BookCreated, x => x.CorrelateById(c => c.Message.CorrelationId).SelectId(c => Guid.NewGuid()));
            Event(() => BookApproved, x => x.CorrelateById(c => c.Message.CorrelationId));
            Initially(
                When(BookCreated)
                    .Then(c =>
                    {
                        Mapper.Map(c.Data, c.Instance);
                        c.Instance.Status = BookStatus.Created;
                        c.Instance.CurrentState = "Created";
                        c.Instance.ExpirationId = Guid.NewGuid();
                    })
                    .Send(TaskManagerQueue, c => new CreateTaskCommand(c.Instance))
                    .Catch<Exception>(binder => binder)
                );
        }

        private Uri TaskManagerQueue(Book instance, IBookCreated data)
        {
            return new Uri($"rabbitmq://domer-ss/book");
        }

        public Event<IBookCreated> BookCreated { get; set; }
        public Event<IBookApproved> BookApproved { get; set; }
    }

    public class CreateTaskCommand
    {
        public CreateTaskCommand(Book instance)
        {
            Book = instance;
            TimeStamp = DateTimeOffset.Now;
        }

        public Book Book { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }

    public class SagaInstanceMap : SagaClassMapping<Book>
    {
        public SagaInstanceMap()
        {
            Property(x => x.RowVersion).IsRowVersion();
        }
    }
}
