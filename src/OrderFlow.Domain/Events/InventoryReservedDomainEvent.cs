using System.Text.Json.Serialization;

namespace OrderFlow.Domain.Events;

/// <summary>
/// Represents the successful reservation of inventory for an order.
/// </summary>
public sealed class InventoryReservedDomainEvent : DomainEvent
{
    public Guid OrderId { get; }

    public Guid ReservationId { get; }

    public InventoryReservedDomainEvent(Guid orderId, Guid reservationId)
    {
        OrderId = orderId;
        ReservationId = reservationId;
    }

    [JsonConstructor]
    public InventoryReservedDomainEvent(Guid eventId,
                                        DateTime occurredAt,
                                        Guid orderId,
                                        Guid reservationId) : base(eventId, occurredAt)
    {
        OrderId = orderId;
        ReservationId = reservationId;
    }
}