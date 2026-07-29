namespace OrderFlow.Api.Contracts.Orders.Requests;

public sealed record CreateOrderItemRequest(Guid ProductId, int Quantity, decimal UnitPrice);
