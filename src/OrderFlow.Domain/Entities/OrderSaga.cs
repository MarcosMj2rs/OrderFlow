using OrderFlow.Domain.Abstractions;
using OrderFlow.Domain.Enumerations;
using OrderFlow.Domain.Exceptions;

namespace OrderFlow.Domain.Entities;

/// <summary>
/// Represents the state of the saga responsible for processing an order.
/// </summary>
public sealed class OrderSaga : Entity
{
    public Guid OrderId { get; private set; }

    public EOrderSagaStatus Status { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    private OrderSaga() { }

    public OrderSaga(Guid orderId)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("OrderId cannot be empty.");

        OrderId = orderId;
        Status = EOrderSagaStatus.STARTED;
        StartedAt = DateTime.UtcNow;
    }

    public void MarkPaymentCompleted()
    {
        if (Status != EOrderSagaStatus.STARTED)
            throw new DomainException("Payment can only be completed when the saga is started.");

        Status = EOrderSagaStatus.PAYMENT_COMPLETED;
    }

    public void MarkInventoryReserved()
    {
        if (Status != EOrderSagaStatus.PAYMENT_COMPLETED)
            throw new DomainException(
                "Inventory can only be reserved after payment is completed.");

        Status = EOrderSagaStatus.INVENTORY_RESERVED;
    }

    public void Complete()
    {
        if (Status != EOrderSagaStatus.INVENTORY_RESERVED)
            throw new DomainException("Saga can only be completed after inventory is reserved.");

        Status = EOrderSagaStatus.COMPLETED;
        CompletedAt = DateTime.UtcNow;
    }

    public void StartCompensation()
    {
        if (Status != EOrderSagaStatus.PAYMENT_COMPLETED &&
            Status != EOrderSagaStatus.INVENTORY_RESERVED)
            throw new DomainException("Saga can only start compensation after payment is completed.");

        Status = EOrderSagaStatus.COMPENSATING;
    }

    public void MarkCompensated()
    {
        if (Status != EOrderSagaStatus.COMPENSATING)
            throw new DomainException("Saga can only be compensated when compensation is in progress.");

        Status = EOrderSagaStatus.COMPENSATED;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        if (Status == EOrderSagaStatus.COMPLETED ||
            Status == EOrderSagaStatus.COMPENSATED)
            throw new DomainException("A finished saga cannot be marked as failed.");

        Status = EOrderSagaStatus.FAILED;
        CompletedAt = DateTime.UtcNow;
    }



}