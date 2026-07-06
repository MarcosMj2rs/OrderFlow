using OrderFlow.Application.Features.Orders.Queries.GetOrderById;
using OrderFlow.Application.Features.Orders.Queries.GetOrders;

namespace OrderFlow.Application.Abstractions.Persistence
{
    public interface IOrderReadRepository
    {
        Task<GetOrderByIdResponse?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<GetOrdersResponse>> GetOrdersAsync(CancellationToken cancellationToken = default);
    }
}
