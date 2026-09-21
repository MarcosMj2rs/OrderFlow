using MediatR;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Features.Sagas.Order.Commands.HandlePaymentCompleted;
using OrderFlow.Infrastructure.Messaging.Consumers;
using OrderFlow.Infrastructure.Messaging.Exceptions;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Connection;
using OrderFlow.Infrastructure.Persistence.Inbox;
using OrderFlow.Worker.Payments.Messages;
using System.Text.Json;

namespace OrderFlow.Worker.Payments.Consumers;

public sealed class PaymentCompletedConsumer : RabbitMqConsumerBase<PaymentCompletedMessage>
{
    private readonly ILogger<PaymentCompletedConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    protected override string QueueName => "orderflow.payment-completed";
    protected override string RetryRoutingKey => "payment.completed.retry";
    protected override string DeadLetterRoutingKey => "payment.completed.dlq";

    public PaymentCompletedConsumer(RabbitMqChannelFactory channelFactory,
                                    IOptions<RabbitMqOptions> options,
                                    ILogger<PaymentCompletedConsumer> logger,
                                    IServiceScopeFactory scopeFactory) : base(channelFactory, options, logger)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ProcessMessageAsync(PaymentCompletedMessage message, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();

        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        IInboxProcessor inboxProcessor = scope.ServiceProvider.GetRequiredService<IInboxProcessor>();

        if (message.EventId == Guid.Empty)
            throw new PermanentMessagingException("PaymentCompletedMessage EventId cannot be empty.");

        if (message.OrderId == Guid.Empty)
            throw new PermanentMessagingException("PaymentCompletedMessage OrderId cannot be empty.");

        if (message.PaymentId == Guid.Empty)
            throw new PermanentMessagingException("PaymentCompletedMessage PaymentId cannot be empty.");

        EInboxProcessingResult result =
            await inboxProcessor.ProcessAsync(message.EventId,
                                              nameof(PaymentCompletedMessage),
                                              JsonSerializer.Serialize(message),
                                              async ct =>
                                              {
                                                  _logger.LogInformation(
                                                      @"Processing payment completed message.
                                                        EventId: {EventId},
                                                        OrderId: {OrderId},
                                                        PaymentId: {PaymentId}",
                                                        message.EventId,
                                                        message.OrderId,
                                                        message.PaymentId);

                                                  await sender.Send(
                                                      new HandlePaymentCompletedCommand(message.OrderId, message.PaymentId),
                                                      ct);
                                              },
                                              cancellationToken);

        if (result == EInboxProcessingResult.ALREADY_PROCESSING)
            throw new TransientMessagingException($"Inbox message '{message.EventId}' is already being processed.");
    }
}