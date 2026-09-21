using OrderFlow.Domain.Entities;

namespace OrderFlow.Domain.Repositories;

public interface IInventoryReservationRepository
{
    Task<InventoryReservation?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task AddAsync(InventoryReservation reservation, CancellationToken cancellationToken = default);
}