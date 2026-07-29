namespace OrderFlow.Api.Contracts.Orders.Responses;

public sealed record CreateOrderResponse(Guid OrderId, Guid CustomerId, decimal TotalAmount);