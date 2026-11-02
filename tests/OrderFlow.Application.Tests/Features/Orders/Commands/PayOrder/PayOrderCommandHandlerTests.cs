
using FluentAssertions;
using Moq;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Features.Orders.Commands.PayOrder;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enumerations;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Tests.Features.Orders.Commands.PayOrder;

public sealed class PayOrderCommandHandlerTests
{
    [Fact]
    public async Task Should_Pay_Order_When_Order_Exists()
    {
        // Arrange
        var order = new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            quantity: 2,
            unitPrice: 100m);

        var saga = new OrderSaga(order.Id);
        saga.MarkPaymentCompleted();
        saga.MarkInventoryReserved();

        var repositoryMock = new Mock<IOrderRepository>();
        var sagaRepositoryMock = new Mock<IOrderSagaRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var transactionContextMock = new Mock<IOrderTransactionContext>();
        var transactionExecutorMock = new Mock<IOrderTransactionExecutor>();

        repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        sagaRepositoryMock
            .Setup(repository => repository.GetByOrderIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(saga);

        transactionContextMock
            .SetupGet(context => context.Orders)
            .Returns(repositoryMock.Object);

        transactionContextMock
            .SetupGet(context => context.OrderSagas)
            .Returns(sagaRepositoryMock.Object);

        transactionContextMock
            .SetupGet(context => context.UnitOfWork)
            .Returns(unitOfWorkMock.Object);

        transactionExecutorMock
            .Setup(executor => executor.ExecuteAsync(
                It.IsAny<Guid>(),
                It.IsAny<Func<IOrderTransactionContext, CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Guid,
                     Func<IOrderTransactionContext, CancellationToken, Task>,
                     CancellationToken>(
                (_, operation, ct) =>
                    operation(transactionContextMock.Object, ct));

        var handler = new PayOrderCommandHandler(
            transactionExecutorMock.Object);

        var command = new PayOrderCommand(order.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        order.Status.Should().Be(OrderStatus.PAID);
        saga.Status.Should().Be(EOrderSagaStatus.COMPLETED);

        transactionExecutorMock.Verify(
            executor => executor.ExecuteAsync(
                order.Id,
                It.IsAny<Func<IOrderTransactionContext, CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Throw_DomainException_When_Order_Does_Not_Exist()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var saga = new OrderSaga(orderId);
        saga.MarkPaymentCompleted();
        saga.MarkInventoryReserved();

        var repositoryMock = new Mock<IOrderRepository>();
        var sagaRepositoryMock = new Mock<IOrderSagaRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var transactionContextMock = new Mock<IOrderTransactionContext>();
        var transactionExecutorMock = new Mock<IOrderTransactionExecutor>();

        repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                orderId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        sagaRepositoryMock
            .Setup(repository => repository.GetByOrderIdAsync(
                orderId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(saga);

        transactionContextMock
            .SetupGet(context => context.Orders)
            .Returns(repositoryMock.Object);

        transactionContextMock
            .SetupGet(context => context.OrderSagas)
            .Returns(sagaRepositoryMock.Object);

        transactionContextMock
            .SetupGet(context => context.UnitOfWork)
            .Returns(unitOfWorkMock.Object);

        transactionExecutorMock
            .Setup(executor => executor.ExecuteAsync(
                It.IsAny<Guid>(),
                It.IsAny<Func<IOrderTransactionContext, CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Guid,
                     Func<IOrderTransactionContext, CancellationToken, Task>,
                     CancellationToken>(
                (_, operation, ct) =>
                    operation(transactionContextMock.Object, ct));

        var handler = new PayOrderCommandHandler(
            transactionExecutorMock.Object);

        var command = new PayOrderCommand(orderId);

        // Act
        Func<Task> act = () =>
            handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("Order not found.");

        unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
