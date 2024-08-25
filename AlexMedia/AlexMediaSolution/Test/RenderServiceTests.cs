using AlexMedia.Interfaces;
using AlexMedia.Services;
using Moq;


namespace AlexMedia.Tests
{
    public class RenderServiceTests
    {
        private readonly Mock<IRenderService> _mockRenderService;

        public RenderServiceTests()
        {
            _mockRenderService = new Mock<IRenderService>();
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldReturnFormattedTemplate()
        {
            // Arrange
            var templatePath = "AlexMedia/AlexMediaSolution/Test/Template.html";
            var template = await File.ReadAllTextAsync(templatePath); // Read the template from the file
            var marketingData = "{\"title\":\"Test Title\"}";
            var expectedOutput = "<h1>Test Title</h1>";

            _mockRenderService.Setup(s => s.RenderTemplateAsync(template, marketingData))
                .ReturnsAsync(expectedOutput);

            var service = new RenderService(); // Assuming RenderService has no dependencies

            // Act
            var result = await service.RenderTemplateAsync(template, marketingData);

            // Assert
            Assert.Equal(expectedOutput, result);
        }
    }
}