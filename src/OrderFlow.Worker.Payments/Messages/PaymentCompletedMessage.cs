namespace OrderFlow.Worker.Payments.Messages;

/// <summary>
/// Represents a payment completed event received from RabbitMQ.
/// </summary>
public sealed record PaymentCompletedMessage(Guid EventId, DateTime OccurredAt, Guid OrderId, Guid PaymentId);