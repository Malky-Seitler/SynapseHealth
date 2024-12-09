using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using SynapseInterview.Code.Models;
using SynapseInterview.Code.Repos;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Synapse.OrdersExample.Tests
{
    public class ProgramTests
    {
        private readonly Mock<LoggingRepo> _mockLoggingRepo;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _mockHttpClient;

        public ProgramTests()
        {
            _mockLoggingRepo = new Mock<LoggingRepo>("mockConnectionString");
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockHttpClient = new HttpClient(_mockHttpMessageHandler.Object);
        }

        [Fact]
        public async Task FetchMedicalEquipmentOrders_SuccessfulFetch_ReturnsOrders()
        {
            // Arrange
            var fakeOrdersJson = JArray.FromObject(new[]
            {
                new { OrderId = 1, Items = new[] { new { Status = "Delivered", Description = "Item1", DeliveryNotification = 0 } } },
                new { OrderId = 2, Items = new[] { new { Status = "Pending", Description = "Item2", DeliveryNotification = 0 } } }
            }).ToString();

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fakeOrdersJson, Encoding.UTF8, "application/json")
            };

            // Mock SendAsync method of HttpMessageHandler
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(response);

            var program = new Program(_mockLoggingRepo.Object, _mockHttpClient);

            // Act
            var orders = await program.FetchMedicalEquipmentOrders();

            // Assert
            Assert.NotNull(orders);
            Assert.Equal(2, orders.Length);
            Assert.Equal("testId", orders[0].OrderId); 
        }

        [Fact]
        public async Task FetchMedicalEquipmentOrders_FailedFetch_ThrowsException()
        {
            // Arrange
            var errorMessage = "Error fetching orders";
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent(errorMessage)
            };

            // Mock SendAsync method of HttpMessageHandler
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(response);

            var program = new Program(_mockLoggingRepo.Object, _mockHttpClient);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => program.FetchMedicalEquipmentOrders());
            Assert.Equal(errorMessage, exception.Message);

            _mockLoggingRepo.Verify(repo =>
                repo.InsertLog(It.Is<Log>(log => log.IsErrorLog && log.Message == errorMessage)), Times.Once);
        }

        [Fact]
        public async Task ProcessOrder_ItemDelivered_SendsAlertAndIncrementsNotification()
        {
            // Arrange
            var order = new Order
            {
                OrderId = "testId",
                Items = new List<Item>
                {
                    new Item { Status = Status.Delivered, Description = "Item1", DeliveryNotification = 0 },
                    new Item { Status = Status.In_Progress, Description = "Item2", DeliveryNotification = 0 }
                }
            };

            var program = new Program(_mockLoggingRepo.Object, _mockHttpClient);

            // Act
            var processedOrder = await program.ProcessOrder(order);

            // Assert
            Assert.NotNull(processedOrder);
            Assert.Equal(1, processedOrder.Items[0].DeliveryNotification);
            Assert.Equal(0, processedOrder.Items[1].DeliveryNotification);

            _mockLoggingRepo.Verify(repo =>
                repo.InsertLog(It.Is<Log>(log => log.Message.Contains("Processing order"))), Times.Once);

            _mockLoggingRepo.Verify(repo =>
                repo.InsertLog(It.Is<Log>(log => log.Message.Contains("Incrementing delivery notification"))), Times.Once);
        }

        [Fact]
        public async Task SendAlertMessage_ValidItem_SendsAlertSuccessfully()
        {
            // Arrange
            var item = new Item
            {
                Description = "TestItem",
                DeliveryNotification = 0
            };

            var orderId = "123";
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            // Mock SendAsync method of HttpMessageHandler
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(response);

            var program = new Program(_mockLoggingRepo.Object, _mockHttpClient);

            // Act
            await program.SendAlertMessage(item, orderId);

            // Assert
            _mockLoggingRepo.Verify(repo =>
                repo.InsertLog(It.Is<Log>(log => log.Message.Contains("Alert sent for delivered item"))), Times.Once);
        }
    }
}
