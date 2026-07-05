using MediatR;

namespace OrderFlow.Application.Features.Orders.Commands.CancelOrder
{
    public sealed record CancelOrderCommand(Guid OrderId) : IRequest;
}
