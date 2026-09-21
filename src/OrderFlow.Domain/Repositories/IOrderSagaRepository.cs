using OrderFlow.Domain.Entities;

namespace OrderFlow.Domain.Repositories;

public interface IOrderSagaRepository
{
    Task<OrderSaga?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task AddAsync(OrderSaga orderSaga, CancellationToken cancellationToken = default);
}