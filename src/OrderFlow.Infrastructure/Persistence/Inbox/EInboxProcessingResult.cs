using System.ComponentModel;

namespace OrderFlow.Infrastructure.Persistence.Inbox;

public enum EInboxProcessingResult
{
    [Description("Processed")]
    PROCESSED = 1,

    [Description("Already Processed")]
    ALREADY_PROCESSED = 2,

    [Description("Already Processing")]
    ALREADY_PROCESSING = 3
}
