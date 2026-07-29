namespace OrderFlow.Api.Contracts.Orders.Requests;

public sealed record CreateOrderRequest(Guid CustomerId, List<CreateOrderItemRequest> Items);