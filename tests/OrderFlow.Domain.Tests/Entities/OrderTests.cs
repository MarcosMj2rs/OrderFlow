using FluentAssertions;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enumerations;
using OrderFlow.Domain.Events;
using OrderFlow.Domain.Exceptions;

namespace OrderFlow.Domain.Tests.Entities
{
    public class OrderTests
    {
        [Fact]
        public void Should_Create_Order_With_Initial_Item()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            // Act
            var order = new Order(customerId, productId, quantity: 2, unitPrice: 150m);

            // Assert
            order.Id.Should().NotBe(Guid.Empty);
            order.CustomerId.Should().Be(customerId);
            order.Status.Should().Be(OrderStatus.PENDING);
            order.Items.Should().HaveCount(1);
            order.TotalAmount.Should().Be(300m);
        }

        [Fact]
        public void Shoud_Increase_Quantity_When_Product_Already_Exists()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var order = new Order(customerId, productId, quantity: 2, unitPrice: 100m);

            // Act
            order.AddItem(productId, quantity: 3, unitPrice: 100m);

            // Assert
            order.Items.Should().HaveCount(1);
            var item = order.Items.Single();
            item.Quantity.Should().Be(5);
            item.UnitPrice.Should().Be(100m);
            item.Total.Should().Be(500m);
            order.TotalAmount.Should().Be(500m);
        }

        [Fact]
        public void Should_Add_New_Item_When_Product_Does_Not_Exist()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var firstProductid = Guid.NewGuid();
            var secondProductId = Guid.NewGuid();

            var order = new Order(customerId, firstProductid, quantity: 2, unitPrice: 100m);

            order.AddItem(secondProductId, quantity: 1, unitPrice: 50m);

            // Act
            order.RemoveItem(secondProductId);

            // Assert
            order.Items.Should().HaveCount(1);
            order.Items.Single().ProductId.Should().Be(firstProductid);
            order.TotalAmount.Should().Be(200m);
        }

        [Fact]
        public void Should_Throw_DomainException_When_Removing_Last_Item()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var order = new Order(customerId, productId, quantity: 1, unitPrice: 100m);

            // Act
            Action act = () => order.RemoveItem(productId);

            // Assert
            act.Should().Throw<DomainException>().WithMessage("An order must contain at least one item.");
        }

        [Fact]
        public void Should_Raise_OrderCreatedDomainEvent_When_Order_Is_Created()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            // Act
            var order = new Order(customerId, productId, quantity: 2, unitPrice: 100m);

            // Assert
            order.DomainEvents.Should().ContainSingle();
            var domainEvent = order.DomainEvents.Single();
            domainEvent.Should().BeOfType<OrderCreatedDomainEvent>();
        }

        [Fact]
        public void Should_Raise_OrderPaidDomainEvent_When_Order_Is_Paid()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var order = new Order(customerId, productId, quantity: 2, unitPrice: 100m);

            order.ClearDomainEvents();

            // Act
            order.Pay();

            // Assert
            order.DomainEvents.Should().ContainSingle();
            var domainEvent = order.DomainEvents.Single();
            domainEvent.Should().BeOfType<OrderPaidDomainEvent>();
        }

        [Fact]
        public void Should_Raise_OrderCancelledDomainEvent_When_Order_Is_Cancelled()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var order = new Order(customerId, productId, quantity: 2, unitPrice: 100m);
            order.ClearDomainEvents();

            // Act
            order.Cancel();

            // Assert
            order.DomainEvents.Should().ContainSingle();
            var domainEvent = order.DomainEvents.Single();
            domainEvent.Should().BeOfType<OrderCancelledDomainEvent>();
        }
    }
}
