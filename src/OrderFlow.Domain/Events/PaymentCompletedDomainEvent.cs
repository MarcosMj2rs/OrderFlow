using System.Text.Json.Serialization;

namespace OrderFlow.Domain.Events;

/// <summary>
/// Represents the successful completion of a payment.
/// </summary>
public sealed class PaymentCompletedDomainEvent : DomainEvent
{
    public Guid OrderId { get; }

    public Guid PaymentId { get; }

    public PaymentCompletedDomainEvent(Guid orderId, Guid paymentId)
    {
        OrderId = orderId;
        PaymentId = paymentId;
    }

    [JsonConstructor]
    public PaymentCompletedDomainEvent(Guid eventId,
                                       DateTime occurredAt,
                                       Guid orderId,
                                       Guid paymentId) : base(eventId, occurredAt)
    {
        OrderId = orderId;
        PaymentId = paymentId;
    }
}