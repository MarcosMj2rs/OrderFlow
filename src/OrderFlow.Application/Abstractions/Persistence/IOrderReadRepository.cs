using OrderFlow.Application.Features.Orders.Queries.GetOrderById;

namespace OrderFlow.Application.Abstractions.Persistence
{
    public interface IOrderReadRepository
    {
        Task<GetOrderByIdResponse?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    }
}
