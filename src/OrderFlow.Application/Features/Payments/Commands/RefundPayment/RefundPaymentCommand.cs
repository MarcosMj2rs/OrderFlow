using MediatR;

namespace OrderFlow.Application.Features.Payments.Commands.RefundPayment;

public sealed record RefundPaymentCommand(Guid OrderId) : IRequest;