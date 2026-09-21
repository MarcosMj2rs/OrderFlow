using System.ComponentModel;

namespace OrderFlow.Domain.Enumerations;

public enum EPaymentStatus
{
    [Description("Completed")]
    COMPLETED = 1,

    [Description("Refunded")]
    REFUNDED = 2
}
