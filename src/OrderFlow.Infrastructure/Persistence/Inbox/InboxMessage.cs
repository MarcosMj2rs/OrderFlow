namespace OrderFlow.Infrastructure.Persistence.Inbox;

public sealed class InboxMessage
{
    public Guid EventId { get; private set; }

    public string Type { get; private set; }

    public string Payload { get; private set; }

    public DateTime ReceivedOnUtc { get; private set; }

    public DateTime? ProcessedOnUtc { get; private set; }

    public string? Error { get; private set; }

    public EInboxMessageStatus Status { get; private set; }

    public DateTime ProcessingStartedOnUtc { get; private set; }

    public InboxMessage(Guid eventId, string type, string payload, DateTime receivedOnUtc)
    {
        EventId = eventId;
        Type = type;
        Payload = payload;
        ReceivedOnUtc = receivedOnUtc;
        Status = EInboxMessageStatus.PROCESSING;
        ProcessingStartedOnUtc = receivedOnUtc;
    }

    public void MarkAsProcessed(DateTime processedOnUtc)
    {
        if (processedOnUtc == default)
            throw new ArgumentException("ProcessedOnUtc must contain a valid date.", nameof(processedOnUtc));

        Status = EInboxMessageStatus.PROCESSED;
        ProcessedOnUtc = processedOnUtc;
        Error = null;
    }

    public void MarkAsFailed(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Error cannot be empty.", nameof(error));

        Error = error;
        ProcessedOnUtc = null;
        Status = EInboxMessageStatus.FAILED;
    }

    public void MarkAsProcessing(DateTime processingStartedOnUtc)
    {
        if (Status != EInboxMessageStatus.FAILED)
            throw new InvalidOperationException("Only failed inbox messages can be moved back to processing.");

        if (processingStartedOnUtc == default)
            throw new ArgumentException("ProcessingStartedOnUtc must contain a valid date.", nameof(processingStartedOnUtc));

        Status = EInboxMessageStatus.PROCESSING;
        ProcessingStartedOnUtc = processingStartedOnUtc;
        Error = null;
        ProcessedOnUtc = null;
    }

    public bool HasProcessingTimedOut(DateTime utcNow, TimeSpan processingTimeout)
    {
        if (Status != EInboxMessageStatus.PROCESSING)
            return false;

        if (utcNow == default)
            throw new ArgumentException("UtcNow must contain a valid date.", nameof(utcNow));

        if (processingTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(processingTimeout), "Processing timeout must be greater than zero.");

        return utcNow - ProcessingStartedOnUtc >= processingTimeout;
    }

    public void RestartProcessing(DateTime processingStartedOnUtc)
    {
        if (Status != EInboxMessageStatus.PROCESSING)
            throw new InvalidOperationException("Only processing inbox messages can restart processing.");

        if (processingStartedOnUtc == default)
            throw new ArgumentException("ProcessingStartedOnUtc must contain a valid date.", nameof(processingStartedOnUtc));

        ProcessingStartedOnUtc = processingStartedOnUtc;
        ProcessedOnUtc = null;
        Error = null;
    }
}
