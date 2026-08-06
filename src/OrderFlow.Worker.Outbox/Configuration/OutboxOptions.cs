namespace OrderFlow.Worker.Outbox.Configuration;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public int BatchSize { get; init; } = 100;

    public int PollingIntervalMilliseconds { get; init; } = 5_000;
}
