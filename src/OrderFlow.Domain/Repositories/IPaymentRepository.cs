using OrderFlow.Domain.Entities;

namespace OrderFlow.Domain.Repositories;

public interface IPaymentRepository
{
    Task<bool> ExistsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
}