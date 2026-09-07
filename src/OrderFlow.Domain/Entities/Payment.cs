using OrderFlow.Domain.Abstractions;
using OrderFlow.Domain.Exceptions;

namespace OrderFlow.Domain.Entities;

public sealed class Payment : Entity
{
    public Guid OrderId { get; private set; }

    public decimal Amount { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private Payment() { }

    public Payment(Guid orderId, decimal amount)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("OrderId cannot be empty.");

        if (amount <= 0)
            throw new DomainException("Payment amount must be greater than zero.");

        OrderId = orderId;
        Amount = amount;
        CreatedAt = DateTime.UtcNow;
    }
}