using Microsoft.Extensions.Options;
using OrderFlow.Infrastructure.Messaging.Consumers;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Connection;
using OrderFlow.Worker.Payments.Messages;

namespace OrderFlow.Worker.Payments.Consumers;

public sealed class OrderCreatedConsumer : RabbitMqConsumerBase<OrderCreatedMessage>
{
    private readonly ILogger<OrderCreatedConsumer> _logger;

    protected override string QueueName => "orderflow.order-created";

    public OrderCreatedConsumer(RabbitMqChannelFactory channelFactory,
                                IOptions<RabbitMqOptions> options,
                                ILogger<OrderCreatedConsumer> logger)
                                : base(channelFactory, options, logger)
    {
        _logger = logger;
    }

    protected override Task ProcessMessageAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
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

        /*
         * Neste primeiro estágio, o processamento do pagamento
         * será representado apenas pelo registro no log.
         */

        return Task.CompletedTask;
    }
}