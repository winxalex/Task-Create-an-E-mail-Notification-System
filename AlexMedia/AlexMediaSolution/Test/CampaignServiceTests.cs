using AlexMedia.Models;
using AlexMedia.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Azure.Messaging.ServiceBus;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AlexMedia.Tests
{
    public class CampaignServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<CampaignService>> _mockLogger;
        private readonly Mock<ServiceBusClient> _mockServiceBusClient;
        private readonly Mock<ServiceBusSender> _mockSender;

        public CampaignServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<CampaignService>>();
            _mockServiceBusClient = new Mock<ServiceBusClient>();
            _mockSender = new Mock<ServiceBusSender>();

            // Setup configuration
            _mockConfiguration.Setup(c => c["ServiceBus:QueueName"]).Returns("TestQueue");

            // Setup ServiceBusClient
            _mockServiceBusClient.Setup(c => c.CreateSender(It.IsAny<string>()))
                .Returns(_mockSender.Object);
        }

        [Fact]
        public async Task StartCampaignAsync_ShouldSendMessage()
        {
            // Arrange
            var campaignService = new CampaignService(_mockConfiguration.Object, _mockServiceBusClient.Object, _mockLogger.Object); // Added logger parameter
            var subject = "Test Subject";
            var sender = "test@example.com";
            var filePath = "Campaign.xml"; // Updated file path
            Assert.True(File.Exists(filePath), "The XML file does not exist."); // Check if file exists
            var xmlContent = File.ReadAllText(filePath); // Read XML content from file
            var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlContent)); // Convert to MemoryStream

            // Act
            await campaignService.StartCampaignAsync(subject, sender, xmlStream);

            // Assert
            _mockSender.Verify(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default), Times.Once);
        }

        // Additional tests can be added here
    }
}