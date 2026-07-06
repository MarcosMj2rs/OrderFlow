using OrderFlow.Domain.Enumerations;

namespace OrderFlow.Application.Features.Orders.Queries.GetOrderById
{
    public sealed record GetOrderByIdResponse
    (
        Guid OrderId,
        Guid CustomerId,
        OrderStatus Status,
        decimal TotalAmount,
        IReadOnlyCollection<GetOrderByIdItemResponse> Items
    );
}
