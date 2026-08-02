namespace OrderFlow.Worker.Payments.Messages;

public sealed record OrderCreatedMessage
(
    Guid EventId,
    DateTime OcurredAt,
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount
);
