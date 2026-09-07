using MediatR;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Repositories;
using OrderFlow.Application.Exceptions;

namespace OrderFlow.Application.Features.Payments.Commands.ProcessPayment;

public sealed class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessPaymentCommandHandler(IPaymentRepository paymentRepository, IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        bool paymentExists = await _paymentRepository.ExistsByOrderIdAsync(request.OrderId, cancellationToken);

        if (paymentExists)
            return;

        var payment = new Payment(request.OrderId, request.Amount);

        await _paymentRepository.AddAsync(payment, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException)
        {
            bool paymentExistsAfterConflict = await _paymentRepository.ExistsByOrderIdAsync(request.OrderId, cancellationToken);

            if (paymentExistsAfterConflict)
                return;

            throw;
        }
    }
}