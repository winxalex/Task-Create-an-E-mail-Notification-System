using AlexMedia.Models;
using AlexMedia.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Azure.Communication.Email;
using System.Threading.Tasks;
using Xunit;
using AlexMedia.Interfaces;
using System.Linq;

namespace AlexMedia.Tests
{
    public class EmailServiceTests
    {
        private readonly Mock<ILogger<EmailService>> _mockLogger;
        private readonly Mock<EmailClient> _mockEmailClient;

        public EmailServiceTests()
        {
            _mockLogger = new Mock<ILogger<EmailService>>();
            _mockEmailClient = new Mock<EmailClient>();
        }

        [Fact]
        public async Task SendEmailAsync_ShouldCallSendAsync()
        {
            // Arrange
            var emailService = new EmailService(_mockLogger.Object, _mockEmailClient.Object);
            var notification = new EmailNotification { Subject = "Test Subject", SenderEmail = "sender@example.com" };
            var recipientAddress = "recipient@example.com";
            var renderedEmail = "<h1>Hello</h1>";
            var sendResult = EmailStatus.Succeeded; // Mocked result
            _mockEmailClient.Setup(c => c.SendAsync(Azure.WaitUntil.Completed, It.IsAny<EmailMessage>(), default))
                .ReturnsAsync(sendResult);

            // Act
            var result = await emailService.SendEmailAsync(notification, recipientAddress, renderedEmail);

            // Assert
            _mockEmailClient.Verify(c => c.SendAsync(It.Is<EmailMessage>(m => m.SenderAddress == notification.SenderEmail && m.Recipients.To.Any(r => r.Address == recipientAddress), default), Times.Once);
            Assert.Equal(EmailStatus.Succeeded, result);
        }
    }
}