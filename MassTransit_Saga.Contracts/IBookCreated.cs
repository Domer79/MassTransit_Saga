using System;

namespace MassTransit_Saga.Contracts
{
    public interface IBookCreated
    {
        Guid CorrelationId { get; set; }    

        string ContactFirstName { get; set; }
        string ContactLastName { get; set; }
        string ContactEmail { get; set; }
        string ContactPhone { get; set; }
        DateTimeOffset ArrivalTime { get; set; }
        DateTimeOffset DepartureTime { get; set; }
        int RatePlanId { get; set; }
        int[] ObjectRoomIds { get; set; }
        string AdditionalRequirements { get; set; }
        DateTimeOffset Created { get; set; }
    }
}