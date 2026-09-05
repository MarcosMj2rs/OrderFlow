namespace OrderFlow.Worker.Payments.Messages;

public sealed record OrderCreatedMessage
(
    Guid EventId,
    DateTime OccurredAt,
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount
);
