namespace OrderFlow.Application.Features.Orders.Commands.CreateOrder
{
    public sealed record CreateOrderResponse
    (
        Guid OrderId,
        Guid CustomerId,
        decimal TotalAmount
    );
}
