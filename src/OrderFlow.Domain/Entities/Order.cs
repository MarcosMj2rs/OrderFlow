using OrderFlow.Domain.Abstractions;
using OrderFlow.Domain.Enumerations;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Events;

namespace OrderFlow.Domain.Entities
{
    public sealed class Order : Entity
    {
        private readonly List<OrderItem> _items = [];

        public Guid CustomerId { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public OrderStatus Status { get; private set; }

        public decimal TotalAmount { get; private set; }

        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        private Order() { }

        public Order(Guid customerId, Guid productId, int quantity, decimal unitPrice)
        {
            if (customerId == Guid.Empty)
                throw new DomainException("CustomerId cannot be empty.");

            CustomerId = customerId;
            CreatedAt = DateTime.UtcNow;
            Status = OrderStatus.PENDING;

            AddItem(productId, quantity, unitPrice);

            RaiseDomainEvent(new OrderCreatedDomainEvent(Id, CustomerId, TotalAmount));
        }

        public void AddItem(Guid productId, int quantity, decimal unitPrice)
        {
            if (Status == OrderStatus.CANCELLED)
                throw new DomainException("Cannot add items to a cancelled order.");

            var existingItem = FindItem(productId);

            if (existingItem is not null)
            {
                var newQuantity = existingItem.Quantity + quantity;
                existingItem.ChangeQuantity(newQuantity);
                existingItem.ChangeUnitPrice(unitPrice);

                RecalculateTotal();

                return;
            }

            var item = new OrderItem(productId, quantity, unitPrice);
            _items.Add(item);

            RecalculateTotal();
        }

        private void RecalculateTotal()
        {
            TotalAmount = _items.Sum(x => x.Total);
        }

        public void RemoveItem(Guid productId)
        {
            if (Status == OrderStatus.CANCELLED)
                throw new DomainException("Cannot remove items from a cancelled order.");

            var item = FindItem(productId);

            if (item is null)
                throw new DomainException("Product not found in the order.");

            if (_items.Count == 1)
                throw new DomainException("An order must contain at least one item.");

            _items.Remove(item);

            RecalculateTotal();
        }

        public void ChangeItemQuantity(Guid productId, int quantity)
        {
            if (Status == OrderStatus.CANCELLED)
                throw new DomainException("Cannot change items from a cancelled order.");

            var item = FindItem(productId);

            if (item is null)
                throw new DomainException("Product not found in the order.");

            item.ChangeQuantity(quantity);

            RecalculateTotal();
        }

        public void Cancel()
        {
            if (Status == OrderStatus.CANCELLED)
                throw new DomainException("Order is already cancelled.");

            if (Status == OrderStatus.PAID)
                throw new DomainException("Paid orders cannot be cancelled.");

            Status = OrderStatus.CANCELLED;

            RaiseDomainEvent(new OrderCancelledDomainEvent(Id, CustomerId, TotalAmount));
        }

        public void Pay()
        {
            if (Status == OrderStatus.CANCELLED)
                throw new DomainException("Cancelled orders cannot be paid.");

            if (Status == OrderStatus.PAID)
                throw new DomainException("Order is already paid.");

            Status = OrderStatus.PAID;

            RaiseDomainEvent(new OrderPaidDomainEvent(Id, CustomerId, TotalAmount));
        }

        private OrderItem? FindItem(Guid productId)
        {
            return _items.FirstOrDefault(x => x.ProductId == productId);
        }
    }
}
