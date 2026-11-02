
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Abstractions.Persistence;

/// <summary>
/// Provides access to repositories and the unit of work
/// associated with a single transactional execution scope.
/// </summary>
public interface IOrderTransactionContext
{
    IOrderRepository Orders { get; }

    IOrderSagaRepository OrderSagas { get; }

    IPaymentRepository Payments { get; }

    IInventoryReservationRepository InventoryReservations { get; }

    IUnitOfWork UnitOfWork { get; }
}
