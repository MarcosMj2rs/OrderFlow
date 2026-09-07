using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Repositories;
using OrderFlow.Infrastructure.Persistence.Context;

namespace OrderFlow.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly OrderFlowDbContext _dbContext;

    public PaymentRepository(OrderFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Payments.AnyAsync(payment => payment.OrderId == orderId, cancellationToken);
    }

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _dbContext.Payments.AddAsync(payment, cancellationToken);
    }
}