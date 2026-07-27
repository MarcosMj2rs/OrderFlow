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
}