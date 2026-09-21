using System.ComponentModel;

namespace OrderFlow.Domain.Enumerations;

/// <summary>
/// Represents the current status of the order saga.
/// </summary>
public enum EOrderSagaStatus
{
    [Description("Started")]
    STARTED = 1,

    [Description("Payment Completed")]
    PAYMENT_COMPLETED = 2,

    [Description("Inventory Reserved")]
    INVENTORY_RESERVED = 3,

    [Description("Completed")]
    COMPLETED = 4,

    [Description("Compensating")]
    COMPENSATING = 5,

    [Description("Compensated")]
    COMPENSATED = 6,

    [Description("Failed")]
    FAILED = 7
}