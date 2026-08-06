using System.Text.Json.Serialization;

namespace OrderFlow.Domain.Events;

public sealed class OrderCancelledDomainEvent : DomainEvent
{
    public Guid OrderId { get; }

    public Guid CustomerId { get; }

    public decimal TotalAmount { get; }

    public OrderCancelledDomainEvent(Guid orderId, Guid customerId, decimal totalAmount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }

    [JsonConstructor]
    public OrderCancelledDomainEvent(Guid eventId, DateTime occurredAt, Guid orderId, Guid customerId, decimal totalAmount)
       : base(eventId, occurredAt)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }
}
