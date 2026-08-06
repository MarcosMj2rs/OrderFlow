using System.Text.Json.Serialization;

namespace OrderFlow.Domain.Events;

public sealed class OrderPaidDomainEvent : DomainEvent
{
    public Guid OrderId { get; }

    public Guid CustomerId { get; }

    public decimal TotalAmount { get; }

    public OrderPaidDomainEvent(Guid orderId, Guid customerId, decimal totalAmount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }

    [JsonConstructor]
    public OrderPaidDomainEvent(Guid eventId, DateTime occurredAt, Guid orderId, Guid customerId, decimal totalAmount)
        : base(eventId, occurredAt)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }
}
