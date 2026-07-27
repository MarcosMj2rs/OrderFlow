using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions.Messaging;
using OrderFlow.Domain.Events;

namespace OrderFlow.Application.Services.Messaging;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IEventPublisher eventPublisher, ILogger<DomainEventDispatcher> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        if (domainEvents.Count == 0)
            return;

        _logger.LogInformation("Dispatching {DomainEventCount} domain event(s).", domainEvents.Count);

        foreach (var domainEvent in domainEvents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
            }
            catch (Exception)
            {
                _logger.LogError("Failed to dispatch domain event {DomainEventType}. EventId: {EventId}",
                                  domainEvent.GetType().Name,
                                  domainEvent.EventId);
                throw;
            }
        }

        _logger.LogInformation("{DomainEventCount} domain event(s) dispatched successfully.", domainEvents.Count);
    }
}
