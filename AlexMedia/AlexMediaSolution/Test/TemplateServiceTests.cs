using AlexMedia.Models;
using AlexMedia.Services;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AlexMedia.Tests
{
    public class TemplateServiceTests
    {
        private readonly Mock<BlobContainerClient> _mockContainerClient;
        private readonly Mock<ILogger<TemplateService>> _mockLogger;

        public TemplateServiceTests()
        {
            _mockContainerClient = new Mock<BlobContainerClient>();
            _mockLogger = new Mock<ILogger<TemplateService>>();
        }

        [Fact]
        public async Task SaveTemplateAsync_ShouldUploadTemplate()
        {
            // Arrange
            var template = new EmailTemplate { ClientId = "clientId", Id = "templateId", Name = "templateName", Content = "templateContent" };
            var mockBlobClient = new Mock<BlobClient>();
            _mockContainerClient.Setup(c => c.GetBlobClient(It.IsAny<string>())).Returns(mockBlobClient.Object);

            // Read HTML content from Template.html
            var htmlFilePath = "Test/Template.html"; // Corrected path to the HTML file
            using var stream = new FileStream(htmlFilePath, FileMode.Open, FileAccess.Read);

            // Update to use the correct parameters for BlobContentInfo
            var blobContentInfo = BlobsModelFactory.BlobContentInfo(ETag.All, DateTimeOffset.UtcNow, null, null, null, 0); // Adjusted parameters
            mockBlobClient.Setup(b => b.UploadAsync(stream, false, default)).ReturnsAsync(Response.FromValue(blobContentInfo, null)); // Updated to use blobContentInfo

            var service = new TemplateService(_mockContainerClient.Object, _mockLogger.Object);

            // Act
            var result = await service.SaveTemplateAsync(template);

            // Assert
            Assert.Equal("clientId/templateId_templateName", result.Id);
            mockBlobClient.Verify(b => b.UploadAsync(stream, false, default), Times.Once);
        }

        [Fact]
        public async Task GetClientTemplatesAsync_ExistingClient_ShouldReturnTemplates()
        {
            // Arrange
            var clientId = "clientId";
            var mockBlobClient = new Mock<BlobClient>();

            var mockBlobItem = new Mock<BlobItem> { CallBase = true };

            mockBlobItem.SetupAllProperties(); // Setup all properties to be accessible

            mockBlobItem.Name = $"{clientId}/templateId_templateName";

            var blobItems = new List<Mock<BlobItem>>
            {
               mockBlobItem
            };

            //TODO: Fix this
            //_mockContainerClient.Setup(c => c.GetBlobsAsync(It.IsAny<string>(), It.IsAny<BlobTraits>())).Returns(GetAsyncBlobItems(blobItems));
            // mockBlobClient.Setup(b => b.DownloadContentAsync()).ReturnsAsync(Response.FromValue(new BlobDownloadResult(new BinaryData("templateContent"), null), null)); // Updated return type

            var service = new TemplateService(_mockContainerClient.Object, _mockLogger.Object);

            // Act
            var result = await service.GetClientTemplatesAsync(clientId);

            // Assert
            Assert.Single(result);
            Assert.Equal("templateId", result[0].Id);
            Assert.Equal("templateName", result[0].Name);
            Assert.Equal("templateContent", result[0].Content);
        }

        private async IAsyncEnumerable<BlobItem> GetAsyncBlobItems(List<BlobItem> items)
        {
            foreach (var item in items)
            {
                yield return item;
            }
        }
    }
}