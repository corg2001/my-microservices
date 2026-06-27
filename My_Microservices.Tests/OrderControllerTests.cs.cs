using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using My_Microservices.Controllers;
using My_Microservices.Models;
using My_Microservices.Services;
using Xunit;

namespace My_Microservices.Tests
{
    public class OrderControllerTests
    {
        private readonly Mock<ICosmosService> _cosmosMock;
        private readonly Mock<IServiceBusService> _serviceBusMock;
        private readonly Mock<DaprClient> _daprMock;
        private readonly OrderController _controller;

        public OrderControllerTests()
        {
            _cosmosMock = new Mock<ICosmosService>();
            _serviceBusMock = new Mock<IServiceBusService>();
            _daprMock = new Mock<DaprClient>();

            _controller = new OrderController(
                _daprMock.Object,
                _cosmosMock.Object
            );
        }

        [Fact]
        public async Task CreateOrder_ReturnsCreatedResult()
        {
            // Arrange
            var request = new CreateOrderRequest("cust-1", "Laptop", 1, 999.99m);
            var order = request.ToOrder();

            _cosmosMock
                .Setup(x => x.SaveOrderAsync(It.IsAny<Order>()))
                .ReturnsAsync(order);

            _daprMock
                .Setup(x => x.SaveStateAsync<Order>(
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Order>(), null, null,
                It.IsAny<CancellationToken>()))
                .Returns(() => Task.CompletedTask);

            _daprMock
                .Setup(x => x.PublishEventAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateOrder(order);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, created.StatusCode);
        }

        [Fact]
        public async Task GetOrder_ReturnsOrder_WhenFoundInDapr()
        {
            // Arrange
            var order = new Order { CustomerId = "cust-1", ProductName = "Laptop" };

            _daprMock
                .Setup(x => x.GetStateAsync<Order>(
                    It.IsAny<string>(), It.IsAny<string>(),
                    null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            // Act
            var result = await _controller.GetOrder(order.Id);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(order, ok.Value);
        }

        [Fact]
        public async Task GetOrder_FallsBackToCosmos_WhenNotInDapr()
        {
            // Arrange
            var order = new Order { CustomerId = "cust-1", ProductName = "Laptop" };

            _daprMock
                .Setup(x => x.GetStateAsync<Order>(
                    It.IsAny<string>(), It.IsAny<string>(),
                    null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order)null!);

            _cosmosMock
                .Setup(x => x.GetOrderAsync(It.IsAny<string>()))
                .ReturnsAsync(order);

            // Act
            var result = await _controller.GetOrder(order.Id);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(order, ok.Value);
        }

        [Fact]
        public async Task GetOrder_ReturnsNotFound_WhenMissingEverywhere()
        {
            // Arrange
            _daprMock
                .Setup(x => x.GetStateAsync<Order>(
                    It.IsAny<string>(), It.IsAny<string>(),
                    null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order)null!);

            _cosmosMock
                .Setup(x => x.GetOrderAsync(It.IsAny<string>()))
                .ReturnsAsync((Order)null!);

            // Act
            var result = await _controller.GetOrder("nonexistent-id");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}