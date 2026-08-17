namespace OrderFlow.Infrastructure.Persistence.Inbox;

public sealed class InboxMessage
{
    public Guid EventId { get; private set; }

    public string Type { get; private set; }

    public string Payload { get; private set; }

    public DateTime ReceivedOnUtc { get; private set; }

    public DateTime? ProcessedOnUtc { get; private set; }

    public string? Error { get; private set; }

    public InboxMessageStatus Status { get; private set; }

    public InboxMessage(Guid eventId, string type, string payload, DateTime receivedOnUtc)
    {
        EventId = eventId;
        Type = type;
        Payload = payload;
        ReceivedOnUtc = receivedOnUtc;
        Status = InboxMessageStatus.PROCESSING;
    }

    public void MarkAsProcessed(DateTime processedOnUtc)
    {
        if (processedOnUtc == default)
            throw new ArgumentException("ProcessedOnUtc must contain a valid date.", nameof(processedOnUtc));

        Status = InboxMessageStatus.PROCESSED;
        ProcessedOnUtc = processedOnUtc;
        Error = null;
    }

    public void MarkAsFailed(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Error cannot be empty.", nameof(error));

        Error = error;
        Status = InboxMessageStatus.FAILED;
    }

    public void MarkAsProcessing()
    {
        if (Status != InboxMessageStatus.FAILED)
            throw new InvalidOperationException("Only failed inbox messages can be moved back to processing.");

        Status = InboxMessageStatus.PROCESSING;
        Error = null;
    }
}
