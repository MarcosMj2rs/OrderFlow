using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Domain.Enumerations;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Features.Payments.Commands.RefundPayment;

public sealed class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderSagaRepository _orderSagaRepository;
    private readonly IOrderTransactionExecutor _transactionExecutor;
    private readonly IUnitOfWork _unitOfWork;

    public RefundPaymentCommandHandler(IPaymentRepository paymentRepository,
                                       IOrderSagaRepository orderSagaRepository,
                                       IOrderTransactionExecutor transactionExecutor,
                                       IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _orderSagaRepository = orderSagaRepository;
        _transactionExecutor = transactionExecutor;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        await _transactionExecutor.ExecuteAsync(
            request.OrderId,
            async ct =>
            {
                var orderSaga = await _orderSagaRepository
                    .GetByOrderIdAsync(request.OrderId, ct);

                if (orderSaga is null)
                    throw new DomainException($"Order saga for order '{request.OrderId}' was not found.");

                var payment = await _paymentRepository.GetByOrderIdAsync(request.OrderId, ct);

                if (payment is null)
                    throw new DomainException($"Payment for order '{request.OrderId}' was not found.");

                // Mantém a idempotência existente.
                if (payment.Status == EPaymentStatus.REFUNDED)
                    return;

                // Estornos somente durante a compensação da Saga.
                if (orderSaga.Status != EOrderSagaStatus.COMPENSATING)
                    throw new DomainException($"Order saga '{request.OrderId}' cannot refund payment from status '{orderSaga.Status}'.");

                payment.Refund();

                await _unitOfWork.SaveChangesAsync(ct);
            },
            cancellationToken);
    }
}