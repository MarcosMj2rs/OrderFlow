using MediatR;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Features.Sagas.Order.Commands.HandleInventoryReserved;
using OrderFlow.Infrastructure.Messaging.Consumers;
using OrderFlow.Infrastructure.Messaging.Exceptions;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Connection;
using OrderFlow.Infrastructure.Persistence.Inbox;
using OrderFlow.Worker.Payments.Messages;
using System.Text.Json;

namespace OrderFlow.Worker.Payments.Consumers;

public sealed class InventoryReservedConsumer
    : RabbitMqConsumerBase<InventoryReservedMessage>
{
    private readonly ILogger<InventoryReservedConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    protected override string QueueName => "orderflow.inventory-reserved";
    protected override string RetryRoutingKey => "inventory.reserved.retry";
    protected override string DeadLetterRoutingKey => "inventory.reserved.dlq";

    public InventoryReservedConsumer(RabbitMqChannelFactory channelFactory,
                                     IOptions<RabbitMqOptions> options,
                                     ILogger<InventoryReservedConsumer> logger,
                                     IServiceScopeFactory scopeFactory) : base(channelFactory, options, logger)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ProcessMessageAsync(InventoryReservedMessage message, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();

        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        IInboxProcessor inboxProcessor = scope.ServiceProvider.GetRequiredService<IInboxProcessor>();

        if (message.EventId == Guid.Empty)
            throw new PermanentMessagingException("InventoryReservedMessage EventId cannot be empty.");

        if (message.OrderId == Guid.Empty)
            throw new PermanentMessagingException("InventoryReservedMessage OrderId cannot be empty.");

        if (message.ReservationId == Guid.Empty)
            throw new PermanentMessagingException("InventoryReservedMessage ReservationId cannot be empty.");

        EInboxProcessingResult result =
            await inboxProcessor.ProcessAsync(message.EventId,
                                              nameof(InventoryReservedMessage),
                                              JsonSerializer.Serialize(message),
                                              async ct =>
                                              {
                                                  _logger.LogInformation(
                                                      @"Processing inventory reserved message.
                                                        EventId: {EventId},
                                                        OrderId: {OrderId},
                                                        ReservationId: {ReservationId}",
                                                        message.EventId,
                                                        message.OrderId,
                                                        message.ReservationId);

                                                  await sender.Send(new HandleInventoryReservedCommand(
                                                                    message.OrderId,
                                                                    message.ReservationId),
                                                                   ct);
                                              },
                                              cancellationToken);

        if (result == EInboxProcessingResult.ALREADY_PROCESSING)
            throw new TransientMessagingException($"Inbox message '{message.EventId}' is already being processed.");
    }
}