using MediatR;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Features.Payments.Commands.ProcessPayment;
using OrderFlow.Infrastructure.Messaging.Consumers;
using OrderFlow.Infrastructure.Messaging.Exceptions;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Connection;
using OrderFlow.Infrastructure.Persistence.Inbox;
using OrderFlow.Worker.Payments.Messages;
using System.Text.Json;

namespace OrderFlow.Worker.Payments.Consumers;

public sealed class OrderCreatedConsumer : RabbitMqConsumerBase<OrderCreatedMessage>
{
    private readonly ILogger<OrderCreatedConsumer> _logger;

    private readonly IServiceScopeFactory _scopeFactory;

    protected override string QueueName => "orderflow.order-created";
    protected override string RetryRoutingKey => "order.created.retry";
    protected override string DeadLetterRoutingKey => "order.created.dlq";

    public OrderCreatedConsumer(RabbitMqChannelFactory channelFactory,
                                IOptions<RabbitMqOptions> options,
                                ILogger<OrderCreatedConsumer> logger,
                                IServiceScopeFactory scopeFactory)
                                : base(channelFactory, options, logger)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ProcessMessageAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();

        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
        IInboxProcessor inboxProcessor = scope.ServiceProvider.GetRequiredService<IInboxProcessor>();

        EInboxProcessingResult result = await inboxProcessor.ProcessAsync(message.EventId,
            nameof(OrderCreatedMessage),
            JsonSerializer.Serialize(message),
            async ct =>
            {
                _logger.LogInformation(@"Processing order created message.
                                         EventId: {EventId},
                                         OrderId: {OrderId},
                                         CustomerId: {CustomerId},
                                         TotalAmount: {TotalAmount}",
                                         message.EventId,
                                         message.OrderId,
                                         message.CustomerId,
                                         message.TotalAmount);

                await sender.Send(new ProcessPaymentCommand(message.OrderId,
                                                            message.TotalAmount),
                                 ct);
            },
            cancellationToken);

        if (result == EInboxProcessingResult.ALREADY_PROCESSING)
            throw new TransientMessagingException($"Inbox message '{message.EventId}' is already being processed.");
    }
}