using Azure.Communication.Email;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WINX.Interfaces;
using WINX.Models;

namespace WINX.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailClient _emailClient;

        // Constructor to initialize logger and email client
        public EmailService(ILogger<EmailService> logger, EmailClient emailClient)
        {
            _logger = logger;
            _emailClient = emailClient;
        }

        /// <summary>
        /// Sends an email asynchronously using the provided notification details and rendered email content.
        /// </summary>
        /// <param name="notification">Email notification details</param>
        /// <param name="recipientAddress">Recipient's email address</param>
        /// <param name="renderedEmail">HTML content of the email</param>
        /// <returns>Status of the email sending operation</returns>
        public async Task<EmailStatus> SendEmailAsync(EmailNotification notification, string recipientAddress, string renderedEmail)
        {
            // Create email content with subject and HTML body
            var emailContent = new EmailContent(notification.Subject)
            {
                Html = renderedEmail
            };

            // Create email message with sender, recipient, and content
            var emailMessage = new EmailMessage(
                senderAddress: notification.SenderEmail,
                recipientAddress: recipientAddress,
                content: emailContent);

            // Send the email asynchronously and wait for completion
            var sendResult = await _emailClient.SendAsync(Azure.WaitUntil.Completed, emailMessage);

            // Map Azure's email status to our custom EmailStatus enum
            var emailStatus = MapEmailStatus(sendResult.Value.Status);

#if DEBUG
            // Log the result of the email sending operation in debug mode
            if (emailStatus == EmailStatus.Succeeded)
            {
                _logger.LogInformation($"Email sent successfully to {recipientAddress}");
            }
            else
            {
                _logger.LogError($"Failed to send email to {recipientAddress}. Status: {emailStatus}");
            }
#endif

            return emailStatus;
        }

        /// <summary>
        /// Maps Azure's EmailSendStatus to our custom EmailStatus enum.
        /// </summary>
        /// <param name="azureStatus">Azure's EmailSendStatus</param>
        /// <returns>Mapped EmailStatus</returns>
        private EmailStatus MapEmailStatus(EmailSendStatus azureStatus)
        {
            // Map Azure's email status to our custom EmailStatus enum
            if (azureStatus == EmailSendStatus.Succeeded)
                return EmailStatus.Succeeded;
            else if (azureStatus == EmailSendStatus.Failed)
                return EmailStatus.Failed;
            else
                throw new ArgumentOutOfRangeException(nameof(azureStatus), azureStatus, "Unexpected EmailSendStatus value");
        }
    }
}