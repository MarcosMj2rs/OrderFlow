using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Features.Orders.Queries.GetOrderById;
using OrderFlow.Application.Features.Orders.Queries.GetOrders;
using OrderFlow.Infrastructure.Persistence.Context;

namespace OrderFlow.Infrastructure.Persistence.Repositories
{
    public sealed class OrderReadRepository : IOrderReadRepository
    {
        private readonly OrderFlowDbContext _context;

        public OrderReadRepository(OrderFlowDbContext context)
        {
            _context = context;
        }

        public async Task<GetOrderByIdResponse?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(order => order.Id == orderId)
                .Select(order => new GetOrderByIdResponse(
                    order.Id,
                    order.CustomerId,
                    order.Status,
                    order.TotalAmount,
                    order.Items
                        .Select(item => new GetOrderByIdItemResponse(
                            item.ProductId,
                            item.Quantity,
                            item.UnitPrice,
                            item.Total))
                        .ToList()))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<GetOrdersResponse>> GetOrdersAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .AsNoTracking()
                .OrderByDescending(order => order.CreatedAt)
                .Select(order => new GetOrdersResponse
                (
                    order.Id,
                    order.CustomerId,
                    order.Status,
                    order.TotalAmount
                ))
                .ToListAsync(cancellationToken);
        }
    }
}
