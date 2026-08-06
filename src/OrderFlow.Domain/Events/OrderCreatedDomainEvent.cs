using System.Text.Json.Serialization;

namespace OrderFlow.Domain.Events;

public sealed class OrderCreatedDomainEvent : DomainEvent
{
    public Guid OrderId { get; }

    public Guid CustomerId { get; }

    public decimal TotalAmount { get; }

    public OrderCreatedDomainEvent(Guid orderId, Guid customerId, decimal totalAmount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }

    [JsonConstructor]
    public OrderCreatedDomainEvent(Guid eventId, DateTime occurredAt, Guid orderId, Guid customerId, decimal totalAmount)
        : base(eventId, occurredAt)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }
}
