using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Features.Orders.Commands.PayOrder
{
    public sealed class PayOrderCommandHandler : IRequestHandler<PayOrderCommand>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PayOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(PayOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

            if (order is null)
                throw new DomainException("Order not found.");

            order.Pay();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
