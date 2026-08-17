using System.ComponentModel;

namespace OrderFlow.Infrastructure.Persistence.Inbox;

public enum EInboxMessageStatus
{
    [Description("Pending")]
    PROCESSING = 1,
    [Description("Processed")]
    PROCESSED = 2,
    [Description("Failed")]
    FAILED = 3
}
