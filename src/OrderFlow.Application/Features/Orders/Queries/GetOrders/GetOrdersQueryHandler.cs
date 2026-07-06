using MediatR;
using OrderFlow.Application.Abstractions.Persistence;

namespace OrderFlow.Application.Features.Orders.Queries.GetOrders
{
    public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IReadOnlyCollection<GetOrdersResponse>>
    {
        private readonly IOrderReadRepository _orderReadRepository;

        public GetOrdersQueryHandler(IOrderReadRepository orderReadRepository)
        {
            _orderReadRepository = orderReadRepository;
        }

        public async Task<IReadOnlyCollection<GetOrdersResponse>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            return await _orderReadRepository.GetOrdersAsync(cancellationToken);
        }
    }
}
