using FluentAssertions;
using Moq;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Features.Orders.Queries.GetOrders;
using OrderFlow.Domain.Enumerations;

namespace OrderFlow.Application.Tests.Features.Orders.Queries.GetOrders
{
    public sealed class GetOrdersQueryHandlerTests
    {
        [Fact]
        public async Task Should_Return_Orders()
        {
            // Arrange
            var repositoryMock = new Mock<IOrderReadRepository>();

            IReadOnlyCollection<GetOrdersResponse> expected =
            [
                new(Guid.NewGuid(), Guid.NewGuid(), OrderStatus.PENDING, 150m),
                new(Guid.NewGuid(), Guid.NewGuid(), OrderStatus.PAID, 300m)
            ];

            repositoryMock
                .Setup(repository => repository.GetOrdersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var handler = new GetOrdersQueryHandler(repositoryMock.Object);

            var query = new GetOrdersQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expected);

            repositoryMock.Verify(repository => repository.GetOrdersAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Return_Empty_Collection_When_No_Orders_Exist()
        {
            // Arrange
            var repositoryMock = new Mock<IOrderReadRepository>();

            IReadOnlyCollection<GetOrdersResponse> expected = Array.Empty<GetOrdersResponse>();

            repositoryMock
                .Setup(repository => repository.GetOrdersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var handler = new GetOrdersQueryHandler(repositoryMock.Object);

            var query = new GetOrdersQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEmpty();

            repositoryMock.Verify(repository => repository.GetOrdersAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
