using MediatR;

namespace OrderFlow.Application.Features.Orders.Queries.GetOrders
{
    public sealed record GetOrdersQuery : IRequest<IReadOnlyCollection<GetOrdersResponse>>;
}
