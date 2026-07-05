using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Features.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var firstItem = request.Items.First();

            var order = new Order(request.CustomerId, firstItem.ProductId, firstItem.Quantity, firstItem.UnitPrice);

            foreach (var item in request.Items.Skip(1))
                order.AddItem(item.ProductId, item.Quantity, item.UnitPrice);

            await _orderRepository.AddAsync(order, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateOrderResponse(order.Id, order.CustomerId, order.TotalAmount);
        }
    }
}
