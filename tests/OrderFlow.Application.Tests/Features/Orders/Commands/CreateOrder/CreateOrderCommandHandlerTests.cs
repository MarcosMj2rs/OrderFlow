using FluentAssertions;
using Moq;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Features.Orders.Commands.CreateOrder;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFlow.Application.Tests.Features.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderCommandHandlerTests
    {
        [Fact]
        public async Task Should_Create_Order_When_Command_Is_Valid()
        {
            // Arrange
            var repositoryMock = new Mock<IOrderRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var handler = new CreateOrderCommandHandler(repositoryMock.Object, unitOfWorkMock.Object);

            var command = new CreateOrderCommand(Guid.NewGuid(),
                new List<CreateOrderItemCommand>
                {
                        new (Guid.NewGuid(), 2, 100m)
                });

            // Act 
            var response = await handler.Handle(command, CancellationToken.None);

            // Assert
            repositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
            unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            response.Should().NotBeNull();
            response.CustomerId.Should().Be(command.CustomerId);
            response.TotalAmount.Should().Be(200m);
            response.OrderId.Should().NotBe(Guid.Empty);
        }
    }
}
