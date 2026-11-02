using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Exceptions;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enumerations;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Features.Payments.Commands.ProcessPayment;

public sealed class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderSagaRepository _orderSagaRepository;
    private readonly IOrderTransactionExecutor _transactionExecutor;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessPaymentCommandHandler(IPaymentRepository paymentRepository,
                                        IOrderSagaRepository orderSagaRepository,
                                        IOrderTransactionExecutor transactionExecutor,
                                        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _orderSagaRepository = orderSagaRepository;
        _transactionExecutor = transactionExecutor;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        await _transactionExecutor.ExecuteAsync(
            request.OrderId,
            async ct =>
            {
                var orderSaga = await _orderSagaRepository
                    .GetByOrderIdAsync(request.OrderId, ct);

                if (orderSaga is null)
                    throw new DomainException(
                        $"Order saga for order '{request.OrderId}' was not found.");

                // Não criar pagamentos após o início da compensação.
                if (orderSaga.Status is EOrderSagaStatus.COMPENSATING or EOrderSagaStatus.COMPENSATED)
                    return;

                // A criação do pagamento só é permitida
                // enquanto a Saga estiver em STARTED.
                if (orderSaga.Status != EOrderSagaStatus.STARTED)
                {
                    throw new DomainException($"Order saga '{request.OrderId}' cannot process payment from status '{orderSaga.Status}'.");
                }

                bool paymentExists = await _paymentRepository.ExistsByOrderIdAsync(request.OrderId, ct);

                if (paymentExists)
                    return;

                var payment = new Payment(request.OrderId, request.Amount);

                await _paymentRepository.AddAsync(payment, ct);

                try
                {
                    await _unitOfWork.SaveChangesAsync(ct);
                }
                catch (UniqueConstraintException)
                {
                    bool paymentExistsAfterConflict = await _paymentRepository.ExistsByOrderIdAsync(request.OrderId, ct);

                    if (paymentExistsAfterConflict)
                        return;

                    throw;
                }
            },
            cancellationToken);
    }
}