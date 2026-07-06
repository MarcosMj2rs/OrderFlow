using FluentAssertions;
using Moq;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Features.Orders.Queries.GetOrderById;
using OrderFlow.Domain.Enumerations;

namespace OrderFlow.Application.Tests.Features.Orders.Queries.GetOrderById
{
    public sealed class GetOrderByIdQueryHandlerTests
    {
        [Fact]
        public async Task Should_Return_Order_When_Order_Exists()
        {
            // Arrange
            var repositoryMock = new Mock<IOrderReadRepository>();

            var response = new GetOrderByIdResponse(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    OrderStatus.PENDING,
                    200m,
                    new List<GetOrderByIdItemResponse> { new(Guid.NewGuid(), 2, 100m, 200m) }
                );

            repositoryMock.Setup(repository => repository
            .GetOrderByIdAsync(response.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

            var handler = new GetOrderByIdQueryHandler(repositoryMock.Object);
            var query = new GetOrderByIdQuery(response.OrderId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(response);

            repositoryMock.Verify(repository => repository
                .GetOrderByIdAsync(response.OrderId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Return_Null_When_Order_Does_Not_Exist()
        {
            // Arrange
            var repositoryMock = new Mock<IOrderReadRepository>();

            var orderId = Guid.NewGuid();

            repositoryMock
                .Setup(repository => repository.GetOrderByIdAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetOrderByIdResponse?)null);

            var handler = new GetOrderByIdQueryHandler(repositoryMock.Object);
            var query = new GetOrderByIdQuery(orderId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();

            repositoryMock.Verify(repository => repository
                .GetOrderByIdAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
