using Newtonsoft.Json;
using OrderFlow.Domain.Enumerations;

namespace OrderFlow.Api.Contracts.Orders.Responses;

public sealed record OrderResponse([property: JsonProperty("orderId")] Guid OrderId,
                                   [property: JsonProperty("customerId")] Guid CustomerId,
                                   [property: JsonProperty("status")] OrderStatus Status,
                                   [property: JsonProperty("totalAmount")] decimal TotalAmount,
                                   [property: JsonProperty("items")] IReadOnlyCollection<OrderItemResponse> Items);

public sealed record OrderSummaryResponse([property: JsonProperty("orderId")] Guid OrderId,
                                          [property: JsonProperty("customerId")] Guid CustomerId,
                                          [property: JsonProperty("status")] OrderStatus Status,
                                          [property: JsonProperty("totalAmount")] decimal TotalAmount);