namespace OrderFlow.Domain.Events
{
    /// <summary>
    /// Represents an event that occurred within the domain.
    /// </summary>
    public interface IDomainEvent
    {
        Guid EventId { get; }

        DateTime OccurredAt { get; }
    }
}
