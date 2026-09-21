using OrderFlow.Domain.Abstractions;
using OrderFlow.Domain.Enumerations;
using OrderFlow.Domain.Events;
using OrderFlow.Domain.Exceptions;

namespace OrderFlow.Domain.Entities;

/// <summary>
/// Represents an inventory reservation associated with an order.
/// </summary>
public sealed class InventoryReservation : Entity
{
    public Guid OrderId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public EInventoryReservationStatus Status { get; private set; }

    private InventoryReservation() { }

    public InventoryReservation(Guid orderId)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("OrderId cannot be empty.");

        OrderId = orderId;
        CreatedAt = DateTime.UtcNow;
        Status = EInventoryReservationStatus.RESERVED;

        RaiseDomainEvent(new InventoryReservedDomainEvent(OrderId, Id));
    }

    public void Release()
    {
        if (Status != EInventoryReservationStatus.RESERVED)
            throw new DomainException("Only reserved inventory reservations can be released.");

        Status = EInventoryReservationStatus.RELEASED;
    }
}