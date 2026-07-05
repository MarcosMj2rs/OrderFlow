using MediatR;

namespace OrderFlow.Application.Features.Orders.Commands.CreateOrder
{
    public sealed record CreateOrderCommand(Guid CustomerId, List<CreateOrderItemCommand> Items) : IRequest<CreateOrderResponse>;

    public sealed record CreateOrderItemCommand(Guid ProductId, int Quantity, decimal UnitPrice);
}
