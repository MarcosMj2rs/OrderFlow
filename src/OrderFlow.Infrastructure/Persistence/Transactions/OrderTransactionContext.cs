
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Infrastructure.Persistence.Transactions;

/// <summary>
/// Exposes repositories and the unit of work
/// resolved from the same dependency injection scope.
/// </summary>
public sealed class OrderTransactionContext : IOrderTransactionContext
{
    public IOrderRepository Orders { get; }

    public IOrderSagaRepository OrderSagas { get; }

    public IPaymentRepository Payments { get; }

    public IInventoryReservationRepository InventoryReservations { get; }

    public IUnitOfWork UnitOfWork { get; }

    public OrderTransactionContext(IOrderRepository orders,
                                   IOrderSagaRepository orderSagas,
                                   IPaymentRepository payments,
                                   IInventoryReservationRepository inventoryReservations,
                                   IUnitOfWork unitOfWork)
    {
        Orders = orders;
        OrderSagas = orderSagas;
        Payments = payments;
        InventoryReservations = inventoryReservations;
        UnitOfWork = unitOfWork;
    }
}
