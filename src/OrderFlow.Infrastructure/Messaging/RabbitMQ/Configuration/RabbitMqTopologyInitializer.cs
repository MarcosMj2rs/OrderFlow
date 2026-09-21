using Microsoft.Extensions.Options;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Connection;
using RabbitMQ.Client;

namespace OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;

public sealed class RabbitMqTopologyInitializer
{
    private const string OrderCreatedQueue = "orderflow.order-created";
    private const string OrderCancelledQueue = "orderflow.order-cancelled";
    private const string OrderPaidQueue = "orderflow.order-paid";

    private const string OrderCreatedRoutingKey = "order.created";
    private const string OrderCancelledRoutingKey = "order.cancelled";
    private const string OrderPaidRoutingKey = "order.paid";

    private const string OrderCreatedRetryQueue = "orderflow.order-created.retry";
    private const string OrderCreatedDeadLetterQueue = "orderflow.order-created.dlq";
    private const string OrderCreatedRetryRoutingKey = "order.created.retry";
    private const string OrderCreatedDeadLetterRoutingKey = "order.created.dlq";

    private const string PaymentCompletedQueue = "orderflow.payment-completed";
    private const string PaymentCompletedRoutingKey = "payment.completed";
    private const string PaymentCompletedRetryQueue = "orderflow.payment-completed.retry";
    private const string PaymentCompletedDeadLetterQueue = "orderflow.payment-completed.dlq";
    private const string PaymentCompletedRetryRoutingKey = "payment.completed.retry";
    private const string PaymentCompletedDeadLetterRoutingKey = "payment.completed.dlq";

    private const string InventoryReservedQueue = "orderflow.inventory-reserved";
    private const string InventoryReservedRoutingKey = "inventory.reserved";

    private const string InventoryReservedRetryQueue = "orderflow.inventory-reserved.retry";
    private const string InventoryReservedDeadLetterQueue = "orderflow.inventory-reserved.dlq";
    private const string InventoryReservedRetryRoutingKey = "inventory.reserved.retry";
    private const string InventoryReservedDeadLetterRoutingKey = "inventory.reserved.dlq";


    private readonly RabbitMqChannelFactory _channelFactory;
    private readonly RabbitMqOptions _options;

    public RabbitMqTopologyInitializer(RabbitMqChannelFactory channelFactory, IOptions<RabbitMqOptions> options)
    {
        _channelFactory = channelFactory;
        _options = options.Value;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using IChannel channel = await _channelFactory.CreateChannelAsync(cancellationToken: cancellationToken);

        await DeclareExchangeAsync(channel, cancellationToken);

        await DeclareQueueAndBindingAsync(channel, OrderCreatedQueue, OrderCreatedRoutingKey, cancellationToken);

        await DeclareQueueAndBindingAsync(channel, OrderCancelledQueue, OrderCancelledRoutingKey, cancellationToken);

        await DeclareQueueAndBindingAsync(channel, OrderPaidQueue, OrderPaidRoutingKey, cancellationToken);

        await DeclareQueueAndBindingAsync(channel, PaymentCompletedQueue, PaymentCompletedRoutingKey, cancellationToken);

        await DeclareQueueAndBindingAsync(channel, InventoryReservedDeadLetterQueue, InventoryReservedDeadLetterRoutingKey, cancellationToken);

        await DeclareQueueAndBindingAsync(channel, InventoryReservedQueue, InventoryReservedRoutingKey, cancellationToken);

        await DeclareOrderCreatedRetryQueueAsync(channel, cancellationToken);

        await DeclarePaymentCompletedRetryQueueAsync(channel, cancellationToken);

        await DeclareInventoryReservedRetryQueueAsync(channel, cancellationToken);

        await DeclareQueueAndBindingAsync(channel, OrderCreatedDeadLetterQueue, OrderCreatedDeadLetterRoutingKey, cancellationToken);

        await DeclareQueueAndBindingAsync(channel, PaymentCompletedDeadLetterQueue, PaymentCompletedDeadLetterRoutingKey, cancellationToken);
    }

    private async Task DeclareExchangeAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(exchange: _options.ExchangeName,
                                           type: ExchangeType.Topic,
                                           durable: true,
                                           autoDelete: false,
                                           arguments: null,
                                           cancellationToken: cancellationToken);
    }

    private async Task DeclareQueueAndBindingAsync(IChannel channel,
                                                   string queueName,
                                                   string routingKey,
                                                   CancellationToken cancellationToken)
    {
        await channel.QueueDeclareAsync(queue: queueName,
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null,
                                        cancellationToken: cancellationToken);

        await channel.QueueBindAsync(queue: queueName,
                                     exchange: _options.ExchangeName,
                                     routingKey: routingKey,
                                     arguments: null,
                                     cancellationToken: cancellationToken);
    }

    private async Task DeclareOrderCreatedRetryQueueAsync(IChannel channel, CancellationToken cancellationToken)
    {
        var arguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = _options.ExchangeName,
            ["x-dead-letter-routing-key"] = OrderCreatedRoutingKey
        };

        await channel.QueueDeclareAsync(queue: OrderCreatedRetryQueue,
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: arguments,
                                        cancellationToken: cancellationToken);

        await channel.QueueBindAsync(queue: OrderCreatedRetryQueue,
                                     exchange: _options.ExchangeName,
                                     routingKey: OrderCreatedRetryRoutingKey,
                                     arguments: null,
                                     cancellationToken: cancellationToken);
    }

    private async Task DeclarePaymentCompletedRetryQueueAsync(IChannel channel, CancellationToken cancellationToken)
    {
        var arguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = _options.ExchangeName,
            ["x-dead-letter-routing-key"] = PaymentCompletedRoutingKey
        };

        await channel.QueueDeclareAsync(queue: PaymentCompletedRetryQueue,
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: arguments,
                                        cancellationToken: cancellationToken);

        await channel.QueueBindAsync(queue: PaymentCompletedRetryQueue,
                                     exchange: _options.ExchangeName,
                                     routingKey: PaymentCompletedRetryRoutingKey,
                                     arguments: null,
                                     cancellationToken: cancellationToken);
    }

    private async Task DeclareInventoryReservedRetryQueueAsync(IChannel channel, CancellationToken cancellationToken)
    {
        var arguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = _options.ExchangeName,
            ["x-dead-letter-routing-key"] = InventoryReservedRoutingKey
        };

        await channel.QueueDeclareAsync(queue: InventoryReservedRetryQueue,
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: arguments,
                                        cancellationToken: cancellationToken);

        await channel.QueueBindAsync(queue: InventoryReservedRetryQueue,
                                     exchange: _options.ExchangeName,
                                     routingKey: InventoryReservedRetryRoutingKey,
                                     arguments: null,
                                     cancellationToken: cancellationToken);
    }
}