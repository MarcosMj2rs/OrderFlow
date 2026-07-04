using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFlow.Domain.Events
{
    public class OrderPaidDomainEvent : DomainEvent
    {
        public Guid OrderId { get; }

        public Guid CustomerId { get; }

        public decimal PaidAmount { get; }

        public OrderPaidDomainEvent(Guid orderId, Guid customerId, decimal paidAmount)
        {
            OrderId = orderId;
            CustomerId = customerId;
            PaidAmount = paidAmount;
        }
    }
}
