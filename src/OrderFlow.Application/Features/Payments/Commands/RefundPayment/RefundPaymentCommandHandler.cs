using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Features.Payments.Commands.RefundPayment;

public sealed class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RefundPaymentCommandHandler(IPaymentRepository paymentRepository, IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

        if (payment is null)
            throw new DomainException($"Payment for order '{request.OrderId}' was not found.");

        payment.Refund();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}