using MediatR;
using OrderFlow.Application.Abstractions.Persistence;

namespace OrderFlow.Application.Features.Orders.Queries.GetOrderById
{
    public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, GetOrderByIdResponse?>
    {
        private readonly IOrderReadRepository _orderReadRepository;

        public GetOrderByIdQueryHandler(IOrderReadRepository orderReadRepository)
        {
            _orderReadRepository = orderReadRepository;
        }

        public async Task<GetOrderByIdResponse?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            return await _orderReadRepository.GetOrderByIdAsync(request.OrderId, cancellationToken);
        }
    }
}
