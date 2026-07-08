using OrderFlow.Domain.Abstractions;
using OrderFlow.Domain.Exceptions;

namespace OrderFlow.Domain.Entities;

public sealed class OrderItem : Entity
{
    public Guid ProductId { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal Total { get; private set; }

    private OrderItem() { }

    internal OrderItem(Guid productId, int quantity, decimal unitPrice)
    {
        ValidateProductId(productId);
        ValidateQuantity(quantity);
        ValidateUnitPrice(unitPrice);

        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;

        RecalculateTotal();
    }

    internal void ChangeQuantity(int quantity)
    {
        ValidateQuantity(quantity);

        Quantity = quantity;

        RecalculateTotal();
    }

    internal void ChangeUnitPrice(decimal unitPrice)
    {
        ValidateUnitPrice(unitPrice);

        UnitPrice = unitPrice;

        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        Total = Quantity * UnitPrice;
    }

    private static void ValidateProductId(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new DomainException("ProductId cannot be empty.");
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
    }

    private static void ValidateUnitPrice(decimal unitPrice)
    {
        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero.");
    }
}