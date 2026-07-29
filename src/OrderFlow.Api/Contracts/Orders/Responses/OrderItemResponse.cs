namespace OrderFlow.Api.Contracts.Orders.Responses;

public sealed record OrderItemResponse(Guid ProductId, int Quantity, decimal UnitPrice, decimal Total);
