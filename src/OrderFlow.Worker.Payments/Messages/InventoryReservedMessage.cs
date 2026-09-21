namespace OrderFlow.Worker.Payments.Messages;

/// <summary>
/// Represents an inventory reserved event received from RabbitMQ.
/// </summary>
public sealed record InventoryReservedMessage(Guid EventId, DateTime OccurredAt, Guid OrderId, Guid ReservationId);