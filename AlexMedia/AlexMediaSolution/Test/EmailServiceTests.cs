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

            // Create EmailContent object
            var emailContent = new EmailContent(notification.Subject)
            {
                Html = renderedEmail
            };

            // Create EmailMessage object using EmailContent
            var emailMessage = new EmailMessage(
                senderAddress: notification.SenderEmail,
                recipientAddress: recipientAddress,
                content: emailContent);

            // Create a mock EmailSendResult
            var mockEmailSendResult = new Mock<EmailSendResult>("operationId", EmailSendStatus.Succeeded);

            // Create a mock EmailSendOperation
            var mockEmailSendOperation = new Mock<EmailSendOperation>("operationId", _mockEmailClient.Object);
            mockEmailSendOperation.Setup(m => m.Value).Returns(mockEmailSendResult.Object);

            // Mock the SendAsync method to return the mocked EmailSendOperation
            _mockEmailClient.Setup(c => c.SendAsync(Azure.WaitUntil.Completed, emailMessage, default))
                .ReturnsAsync(mockEmailSendOperation.Object);

            // Act
            var result = await emailService.SendEmailAsync(notification, recipientAddress, renderedEmail);

            // Assert
            Assert.Equal(EmailStatus.Succeeded, result);
        }
    }
}