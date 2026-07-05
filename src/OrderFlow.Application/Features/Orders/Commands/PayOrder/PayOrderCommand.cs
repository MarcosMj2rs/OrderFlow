using MediatR;

namespace OrderFlow.Application.Features.Orders.Commands.PayOrder
{
    public sealed record PayOrderCommand(Guid OrderId) : IRequest;
}
