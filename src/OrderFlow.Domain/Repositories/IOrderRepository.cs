using OrderFlow.Domain.Entities;

namespace OrderFlow.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid Id, CancellationToken cancellationToken = default);

        Task AddAsync(Order order, CancellationToken cancellationToken = default);

        Task RemoveAsync(Order order, CancellationToken cancellationToken = default);
    }
}
