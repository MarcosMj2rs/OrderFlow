using FluentAssertions;
using Moq;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Features.Orders.Commands.CancelOrder;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enumerations;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Tests.Features.Orders.Commands.CancelOrder
{
    public sealed class CancelOrderCommandHandlerTests
    {
        public async Task Should_Cancel_Order_When_Order_Exists()
        {
            // Arrange
            var order = new Order(Guid.NewGuid(), Guid.NewGuid(), quantity: 1, unitPrice: 100m);

            var repositoryMock = new Mock<IOrderRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            repositoryMock.Setup(repository => repository
                .GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var handler = new CancelOrderCommandHandler(repositoryMock.Object, unitOfWorkMock.Object);

            var command = new CancelOrderCommand(order.Id);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            order.Status.Should().Be(OrderStatus.CANCELLED);

            unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Throw_DomainException_When_Order_Does_Not_Exist()
        {
            // Arrange
            var repositoryMock = new Mock<IOrderRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            repositoryMock
                .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);

            var handler = new CancelOrderCommandHandler(repositoryMock.Object, unitOfWorkMock.Object);

            var command = new CancelOrderCommand(Guid.NewGuid());

            // Act
            var act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Order not found.");

            unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
