namespace OrderFlow.Application.Features.Orders.Queries.GetOrderById
{
    public sealed record GetOrderByIdItemResponse
    (
        Guid ProductId,
        int Quantity,
        decimal UnitPrice,
        decimal Total
    );
}
