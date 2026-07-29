using AutoMapper;
using OrderFlow.Api.Contracts.Orders.Requests;
using OrderFlow.Api.Contracts.Orders.Responses;
using OrderFlow.Application.Features.Orders.Commands.CreateOrder;
using OrderFlow.Application.Features.Orders.Queries.GetOrderById;
using OrderFlow.Application.Features.Orders.Queries.GetOrders;
using ApiCreateOrderResponse = OrderFlow.Api.Contracts.Orders.Responses.CreateOrderResponse;
using ApplicationCreateOrderResponse = OrderFlow.Application.Features.Orders.Commands.CreateOrder.CreateOrderResponse;

namespace OrderFlow.Api.Mappings;

public sealed class OrdersProfile : Profile
{
    public OrdersProfile()
    {
        CreateMap<CreateOrderItemRequest, CreateOrderItemCommand>();

        CreateMap<CreateOrderRequest, CreateOrderCommand>();

        CreateMap<ApplicationCreateOrderResponse, ApiCreateOrderResponse>();

        CreateMap<GetOrderByIdItemResponse, OrderItemResponse>();

        CreateMap<GetOrderByIdResponse, OrderResponse>();

        CreateMap<GetOrdersResponse, OrderSummaryResponse>();
    }
}