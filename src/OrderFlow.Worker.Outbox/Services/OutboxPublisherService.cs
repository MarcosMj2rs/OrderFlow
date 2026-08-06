using OrderFlow.Application.Abstractions.Messaging;
using OrderFlow.Domain.Events;
using OrderFlow.Infrastructure.Persistence.Outbox;
using System.Text.Json;

namespace OrderFlow.Worker.Outbox.Services;

public sealed class OutboxPublisherService
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOutboxEventTypeRegistry _eventTypeRegistry;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<OutboxPublisherService> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OutboxPublisherService(IOutboxRepository outboxRepository,
                                  IOutboxEventTypeRegistry eventTypeRegistry,
                                  IEventPublisher eventPublisher,
                                  ILogger<OutboxPublisherService> logger)
    {
        _outboxRepository = outboxRepository;
        _eventTypeRegistry = eventTypeRegistry;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task PublishPendingMessagesAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<OutboxMessage> messages = await _outboxRepository.GetUnprocessedAsync(batchSize, cancellationToken);

        foreach (OutboxMessage outboxMessage in messages)
            await PublishMessageAsync(outboxMessage, cancellationToken);

        await _outboxRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task PublishMessageAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        try
        {
            Type eventType = _eventTypeRegistry.Resolve(outboxMessage.Type);

            object? deserializedEvent = JsonSerializer.Deserialize(outboxMessage.Payload, eventType, SerializerOptions);

            if (deserializedEvent is not IDomainEvent domainEvent)
                throw new JsonException($"Outbox message '{outboxMessage.Id}' could not be deserialized as {nameof(IDomainEvent)}.");

            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);

            outboxMessage.MarkAsProcessed(DateTime.UtcNow);

            _logger.LogInformation(@"Outbox message published successfully. 
                                     OutboxMessageId: {OutboxMessageId}, 
                                     EventType: {EventType},
                                     EventId: {EventId}",
                                     outboxMessage.Id,
                                     outboxMessage.Type,
                                     domainEvent.EventId);
        }
        catch (Exception exception)
        {
            outboxMessage.SetError(exception.Message);

            _logger.LogError(exception,
                             "Failed to publish outbox message. OutboxMessageId: {OutboxMessageId}, EventType: {EventType}",
                             outboxMessage.Id,
                             outboxMessage.Type);
        }
    }
}