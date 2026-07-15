using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Repositories;
using OrderFlow.Infrastructure.Persistence.Context;

namespace OrderFlow.Infrastructure.Persistence.Repositories
{
    public sealed class OrderRepository : IOrderRepository
    {
        private readonly OrderFlowDbContext _context;

        public OrderRepository(OrderFlowDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include("_items")
                .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
        }

        public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            await _context.Orders.AddAsync(order, cancellationToken);
        }

        public Task RemoveAsync(Order order, CancellationToken cancellationToken = default)
        {
            _context.Orders.Remove(order);
            return Task.CompletedTask;
        }
    }
}
