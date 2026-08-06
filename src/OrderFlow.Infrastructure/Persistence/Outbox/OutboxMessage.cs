namespace OrderFlow.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }

    public string Type { get; private set; }

    public string Payload { get; private set; }

    public DateTime? OccurredOnUtc { get; private set; }

    public DateTime? ProcessedOnUtc { get; private set; }

    public string? Error { get; private set; }

    private OutboxMessage()
    {
        Type = string.Empty;
        Payload = string.Empty;
    }

    public OutboxMessage(Guid id, string type, string payload, DateTime? occurredOnUtc = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Outbox message id cannot be empty.", nameof(id));

        if (String.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Outbox message type cannot be empty.", nameof(type));

        if (String.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Outbox message payload cannot be empty.", nameof(payload));

        if (occurredOnUtc == default)
            throw new ArgumentException("OccurredOnUtc must contain a valid date.", nameof(occurredOnUtc));

        Id = id;
        Type = type;
        Payload = payload;
        OccurredOnUtc = occurredOnUtc ?? DateTime.UtcNow;
    }

    public void MarkAsProcessed(DateTime processedOnUtc)
    {
        if (ProcessedOnUtc is not null)
            throw new InvalidOperationException("Outbox message has already been processed.");

        if (processedOnUtc == default)
            throw new ArgumentException("ProcessedOnUtc must contain a valid date.", nameof(processedOnUtc));

        ProcessedOnUtc = processedOnUtc;
        Error = null;
    }

    public void SetError(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Error message cannot be empty.", nameof(error));
        Error = error;
    }
}
