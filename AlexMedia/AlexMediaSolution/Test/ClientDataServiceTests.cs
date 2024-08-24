using AlexMedia.Models;
using AlexMedia.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Container = Microsoft.Azure.Cosmos.Container;

namespace AlexMedia.Tests
{
    public class ClientDataServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<ClientDataService>> _mockLogger;
        private readonly Mock<CosmosClient> _mockCosmosClient;
        private readonly Mock<Container> _mockContainer;

        public ClientDataServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<ClientDataService>>();
            _mockCosmosClient = new Mock<CosmosClient>();
            _mockContainer = new Mock<Container>();

            // Setup configuration
            _mockConfiguration.Setup(c => c["CosmosDB:ConnectionString"]).Returns("TestConnectionString");
            _mockConfiguration.Setup(c => c["CosmosDB:DatabaseName"]).Returns("TestDatabase");
            _mockConfiguration.Setup(c => c["CosmosDB:ContainerName"]).Returns("TestContainer");

            // Setup CosmosClient
            _mockCosmosClient.Setup(c => c.GetContainer(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_mockContainer.Object);
        }

        [Fact]
        public async Task UpsertClientDataAsync_ShouldCallUpsertItemAsync()
        {
            // Arrange
            var clientData = new ClientData { Id = "testId", FirstName = "Test", LastName = "Client" };
            var mockResponse = new Mock<ItemResponse<ClientData>>();
            mockResponse.Setup(r => r.Resource).Returns(clientData);
            _mockContainer.Setup(c => c.UpsertItemAsync(It.IsAny<ClientData>(), It.IsAny<PartitionKey>(), null, default))
                .ReturnsAsync(mockResponse.Object);

            var service = new ClientDataService(_mockContainer.Object, _mockLogger.Object);

            // Act
            await service.UpsertClientDataAsync(clientData);

            // Assert
            _mockContainer.Verify(c => c.UpsertItemAsync(
                It.Is<ClientData>(cd => cd.Id == clientData.Id && cd.FirstName == clientData.FirstName && cd.LastName == clientData.LastName),
                It.Is<PartitionKey>(pk => pk.ToString() == clientData.Id),
                null,
                default
            ), Times.Once);
        }

        [Fact]
        public async Task GetClientDataAsync_ExistingClient_ShouldReturnClientData()
        {
            // Arrange
            var clientId = "existingClientId";
            var clientData = new ClientData { Id = clientId, FirstName = "Existing", LastName = "Client" };
            var mockResponse = new Mock<ItemResponse<ClientData>>();
            mockResponse.Setup(r => r.Resource).Returns(clientData);
            _mockContainer.Setup(c => c.ReadItemAsync<ClientData>(clientId, It.IsAny<PartitionKey>(), null, default))
                .ReturnsAsync(mockResponse.Object);

            var service = new ClientDataService(_mockContainer.Object, _mockLogger.Object);

            // Act
            var result = await service.GetClientDataAsync(clientId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(clientId, result.Id);
            Assert.Equal("Existing", result.FirstName);
            Assert.Equal("Client", result.LastName);
        }

        [Fact]
        public async Task GetClientDataAsync_NonExistingClient_ShouldReturnNull()
        {
            // Arrange
            var clientId = "nonExistingClientId";
            _mockContainer.Setup(c => c.ReadItemAsync<ClientData>(clientId, It.IsAny<PartitionKey>(), null, default))
                .ThrowsAsync(new CosmosException("Not Found", System.Net.HttpStatusCode.NotFound, 0, "", 0));

            var service = new ClientDataService(_mockContainer.Object, _mockLogger.Object);

            // Act
            var result = await service.GetClientDataAsync(clientId);

            // Assert
            Assert.Null(result);
        }
    }
}