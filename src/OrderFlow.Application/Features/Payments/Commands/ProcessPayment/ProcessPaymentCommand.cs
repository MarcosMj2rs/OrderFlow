using MediatR;

namespace OrderFlow.Application.Features.Payments.Commands.ProcessPayment;

public sealed record ProcessPaymentCommand(Guid OrderId, decimal Amount) : IRequest;