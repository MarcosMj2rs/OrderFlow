using OrderFlow.Domain.Enumerations;

namespace OrderFlow.Application.Features.Orders.Queries.GetOrders
{
    public sealed record GetOrdersResponse(
        Guid OrderId,
        Guid CustomerId,
        OrderStatus Status,
        decimal TotalAmount);
}
