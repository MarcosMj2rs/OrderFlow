namespace OrderFlow.Domain.Events
{
    public sealed class OrderPaidDomainEvent : DomainEvent
    {
        public Guid OrderId { get; }

        public Guid CustomerId { get; }

        public decimal TotalAmount { get; }

        public OrderPaidDomainEvent(Guid orderId, Guid customerId, decimal totalAmount)
        {
            OrderId = orderId;
            CustomerId = customerId;
            TotalAmount = totalAmount;
        }
    }
}
