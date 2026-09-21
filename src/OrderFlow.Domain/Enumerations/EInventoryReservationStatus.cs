using System.ComponentModel;

namespace OrderFlow.Domain.Enumerations;

/// <summary>
/// Represents the status of an inventory reservation.
/// </summary>
public enum EInventoryReservationStatus
{
    [Description("Reserved")]
    RESERVED = 1,

    [Description("Released")]
    RELEASED = 2
}