using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Repositories;
using OrderFlow.Infrastructure.Persistence.Context;

namespace OrderFlow.Infrastructure.Persistence.Repositories;

public sealed class OrderSagaRepository : IOrderSagaRepository
{
    private readonly OrderFlowDbContext _dbContext;

    public OrderSagaRepository(OrderFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderSaga?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.OrderSagas.FirstOrDefaultAsync(saga => saga.OrderId == orderId, cancellationToken);
    }

    public async Task AddAsync(OrderSaga orderSaga, CancellationToken cancellationToken = default)
    {
        await _dbContext.OrderSagas.AddAsync(orderSaga, cancellationToken);
    }
}