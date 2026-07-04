namespace OrderFlow.Domain.Events
{
    public abstract class DomainEvent : IDomainEvent
    {
        public Guid EventId { get; }

        public DateTime OccurredAt { get; }

        protected DomainEvent()
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
        }
    }
}
