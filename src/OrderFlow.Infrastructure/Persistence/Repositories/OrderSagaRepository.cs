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
        var orderSaga = await _dbContext.OrderSagas.FirstOrDefaultAsync(saga => saga.OrderId == orderId, cancellationToken);

        if (orderSaga is null)
            return null;

        await _dbContext.Entry(orderSaga).ReloadAsync(cancellationToken);

        return orderSaga;
    }

    public async Task AddAsync(OrderSaga orderSaga, CancellationToken cancellationToken = default)
    {
        await _dbContext.OrderSagas.AddAsync(orderSaga, cancellationToken);
    }
}