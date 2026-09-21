using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Repositories;
using OrderFlow.Infrastructure.Persistence.Context;

namespace OrderFlow.Infrastructure.Persistence.Repositories;

public sealed class InventoryReservationRepository : IInventoryReservationRepository
{
    private readonly OrderFlowDbContext _dbContext;

    public InventoryReservationRepository(OrderFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InventoryReservation?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.InventoryReservations.FirstOrDefaultAsync(reservation => reservation.OrderId == orderId, cancellationToken);
    }

    public async Task AddAsync(InventoryReservation reservation, CancellationToken cancellationToken = default)
    {
        await _dbContext.InventoryReservations.AddAsync(reservation, cancellationToken);
    }
}