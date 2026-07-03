using System.ComponentModel;

namespace OrderFlow.Domain.Enumerations
{
    public enum OrderStatus
    {
        [Description("Pending")]
        PENDING = 1,
        [Description("Paid")]
        PAID = 2,
        [Description("Cancelled")]
        CANCELLED = 3
    }
}
