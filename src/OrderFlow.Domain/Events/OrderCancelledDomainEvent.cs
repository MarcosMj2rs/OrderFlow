using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFlow.Domain.Events
{
    public class OrderCancelledDomainEvent : DomainEvent
    {
        public Guid OrderId { get; }

        public Guid CustomerId { get; }

        public decimal TotalAmount { get; }

        public OrderCancelledDomainEvent(Guid orderId, Guid customerId, decimal totalAmount)
        {
            OrderId = orderId;
            CustomerId = customerId;
            TotalAmount = totalAmount;
        }
    }
}
