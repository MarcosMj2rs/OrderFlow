namespace OrderFlow.Domain.Events;

public abstract class DomainEvent : IDomainEvent
{
    public Guid EventId { get; }

    public DateTime OccurredAt { get; }

    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
    }

    protected DomainEvent(Guid eventId, DateTime occurredAt)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("EventId cannot be empty.", nameof(eventId));

        if (occurredAt == default)
            throw new ArgumentException("OccurredAt must contain a valid date.", nameof(occurredAt));

        EventId = eventId;
        OccurredAt = occurredAt;
    }
}
