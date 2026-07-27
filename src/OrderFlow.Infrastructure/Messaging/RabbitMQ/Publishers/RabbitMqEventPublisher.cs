using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Abstractions.Messaging;
using OrderFlow.Domain.Events;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Configuration;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Connection;
using OrderFlow.Infrastructure.Messaging.RabbitMQ.Routing;
using RabbitMQ.Client;
using System.Text.Json;

namespace OrderFlow.Infrastructure.Messaging.RabbitMQ.Publishers;

public sealed class RabbitMqEventPublisher : IEventPublisher
{
    private readonly RabbitMqChannelFactory _channelFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly IRabbitMqRoutingKeyResolver _routingKeyResolver;

    public RabbitMqEventPublisher(RabbitMqChannelFactory channelFactory,
                                  IOptions<RabbitMqOptions> options,
                                  IRabbitMqRoutingKeyResolver routingKeyResolver,
                                  ILogger<RabbitMqEventPublisher> logger)
    {
        _channelFactory = channelFactory;
        _options = options.Value;
        _routingKeyResolver = routingKeyResolver;
        _logger = logger;
    }

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent, nameof(domainEvent));

        string routingKey = _routingKeyResolver.Resolve(domainEvent);

        byte[] messageBody = JsonSerializer.SerializeToUtf8Bytes(domainEvent, domainEvent.GetType());

        BasicProperties properties = new()
        {
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = domainEvent.EventId.ToString(),
            Type = domainEvent.GetType().Name,
            Timestamp = new AmqpTimestamp(new DateTimeOffset(domainEvent.OccurredAt).ToUnixTimeSeconds())
        };

        await using IChannel channel = await _channelFactory.CreateChannelAsync(publisherConfirmationsEnabled: true, cancellationToken);

        try
        {
            await channel.BasicPublishAsync(
                           exchange: _options.ExchangeName,
                           routingKey: routingKey,
                           mandatory: true,
                           basicProperties: properties,
                           body: messageBody,
                           cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Domain event {EventType} published to exchange {ExchangeName} using routing key {RoutingKey}. EventId: {EventId}",
                domainEvent.GetType().Name,
                _options.ExchangeName,
                routingKey,
                domainEvent.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                           ex,
                           "Failed to publish domain event {EventType}. EventId: {EventId}",
                           domainEvent.GetType().Name,
                           domainEvent.EventId);

            throw;
        }
    }
}
