
using FluentAssertions;
using Moq;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Abstractions.Sagas;
using OrderFlow.Application.Features.Orders.Commands.CancelOrder;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enumerations;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Repositories;

namespace OrderFlow.Application.Tests.Features.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<IOrderSagaRepository> _sagaRepositoryMock = new();
    private readonly Mock<IOrderSagaOrchestrator> _orchestratorMock = new();
    private readonly Mock<IOrderTransactionExecutor> _transactionExecutorMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    public CancelOrderCommandHandlerTests()
    {
        // Executa efetivamente o callback transacional.
        _transactionExecutorMock
            .Setup(executor => executor.ExecuteAsync(
                It.IsAny<Guid>(),
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Guid, Func<CancellationToken, Task>, CancellationToken>(
                (_, operation, ct) => operation(ct));
    }

    private CancelOrderCommandHandler CreateHandler()
    {
        return new CancelOrderCommandHandler(
            _orderRepositoryMock.Object,
            _sagaRepositoryMock.Object,
            _orchestratorMock.Object,
            _transactionExecutorMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Should_Cancel_Order_When_Order_Exists_Without_Saga()
    {
        // Arrange
        var order = new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            quantity: 1,
            unitPrice: 100m);

        _orderRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _sagaRepositoryMock
            .Setup(repository => repository.GetByOrderIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderSaga?)null);

        var handler = CreateHandler();

        // Act
        await handler.Handle(
            new CancelOrderCommand(order.Id),
            CancellationToken.None);

        // Assert
        order.Status.Should().Be(OrderStatus.CANCELLED);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _orchestratorMock.Verify(
            orchestrator => orchestrator.CancelAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_Throw_DomainException_When_Order_Does_Not_Exist()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                orderId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var handler = CreateHandler();

        // Act
        Func<Task> act = () => handler.Handle(
            new CancelOrderCommand(orderId),
            CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("Order not found.");

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _orchestratorMock.Verify(
            orchestrator => orchestrator.CancelAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_Delegate_Cancellation_To_Orchestrator_When_Saga_Exists()
    {
        // Arrange
        var order = new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            quantity: 1,
            unitPrice: 100m);

        var saga = new OrderSaga(order.Id);

        _orderRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _sagaRepositoryMock
            .Setup(repository => repository.GetByOrderIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(saga);

        var handler = CreateHandler();

        // Act
        await handler.Handle(
            new CancelOrderCommand(order.Id),
            CancellationToken.None);

        // Assert
        _orchestratorMock.Verify(
            orchestrator => orchestrator.CancelAsync(
                order.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);

        // O Handler delega; não cancela diretamente.
        order.Status.Should().Be(OrderStatus.PENDING);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_Not_Cancel_Again_When_Order_Is_Already_Cancelled_Without_Saga()
    {
        // Arrange
        var order = new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            quantity: 1,
            unitPrice: 100m);

        order.Cancel();

        _orderRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _sagaRepositoryMock
            .Setup(repository => repository.GetByOrderIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderSaga?)null);

        var handler = CreateHandler();

        // Act
        await handler.Handle(
            new CancelOrderCommand(order.Id),
            CancellationToken.None);

        // Assert
        order.Status.Should().Be(OrderStatus.CANCELLED);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _orchestratorMock.Verify(
            orchestrator => orchestrator.CancelAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_Reject_Paid_Order_Without_Saga()
    {
        // Arrange
        var order = new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            quantity: 1,
            unitPrice: 100m);

        order.Pay();

        _orderRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _sagaRepositoryMock
            .Setup(repository => repository.GetByOrderIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderSaga?)null);

        var handler = CreateHandler();

        // Act
        Func<Task> act = () => handler.Handle(
            new CancelOrderCommand(order.Id),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();

        order.Status.Should().Be(OrderStatus.PAID);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
