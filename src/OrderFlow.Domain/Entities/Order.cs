using OrderFlow.Domain.Abstractions;
using OrderFlow.Domain.Enumerations;

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

        public Order(Guid customerId)
        {
            if (customerId == Guid.Empty)
                throw new ArgumentException("CustomerId cannot be empty.", nameof(customerId));

            CustomerId = customerId;
            CreatedAt = DateTime.UtcNow;
            Status = OrderStatus.PENDING;
            TotalAmount = 0;
        }

        public void AddItem(Guid productId, int quantity, decimal unitPrice)
        {
            if (Status == OrderStatus.CANCELLED)
                throw new InvalidOperationException("Cannot add items to a cancelled order.");

            var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

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

        public void RemoverItem(Guid productId)
        {
            if (Status == OrderStatus.CANCELLED)
                throw new InvalidOperationException("Cannot remove items from a cancelled order.");

            var item = _items.FirstOrDefault(x => x.ProductId == productId);

            if (item is null)
                throw new InvalidOperationException("Product not found in the order.");

            if (_items.Count == 1)
                throw new InvalidOperationException("An order must contain at least one item.");

            _items.Remove(item);

            RecalculateTotal();
        }

        public void ChangeItemQuantity(Guid productId, int quantity)
        {
            if (Status == OrderStatus.CANCELLED)
                throw new InvalidOperationException("Cannot change items from a cancelled order.");

            var item = _items.FirstOrDefault(x => x.ProductId == productId);

            if (item is null)
                throw new InvalidOperationException("Product not found in the order.");

            item.ChangeQuantity(quantity);

            RecalculateTotal();

        }
    }
}
