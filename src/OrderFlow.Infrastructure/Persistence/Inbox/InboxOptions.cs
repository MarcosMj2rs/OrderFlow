namespace OrderFlow.Infrastructure.Persistence.Inbox;

public sealed class InboxOptions
{
    public const string SectionName = "Inbox";

    public int ProcessingTimeoutMinutes { get; init; } = 5;
}
