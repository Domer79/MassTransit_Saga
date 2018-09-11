using System;

namespace MassTransit_Saga.Contracts
{
    public interface IBookApproved
    {
        Guid CorrelationId { get; set; }
    }
}