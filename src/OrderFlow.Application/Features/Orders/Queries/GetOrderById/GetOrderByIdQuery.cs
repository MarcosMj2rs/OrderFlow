using MediatR;

namespace OrderFlow.Application.Features.Orders.Queries.GetOrderById
{
    public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<GetOrderByIdResponse?>;
}
