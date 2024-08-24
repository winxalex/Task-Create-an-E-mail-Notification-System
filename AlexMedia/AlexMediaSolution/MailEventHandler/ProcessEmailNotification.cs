using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using WINX.Interfaces;
using WINX.Models;

namespace MailerApp
{
    public class ProcessEmailNotification
    {
        private readonly ILogger<ProcessEmailNotification> _logger;
        private readonly IClientDataService _clientDataService;
        private readonly ITemplateService _templateService;
        private readonly IRenderService _renderService;
        private readonly IEmailService _emailService;

        public ProcessEmailNotification(
            ILogger<ProcessEmailNotification> logger,
            IClientDataService clientDataService,
            ITemplateService templateService,
            IRenderService renderService,
            IEmailService emailService)
        {
            _logger = logger;
            _clientDataService = clientDataService;
            _templateService = templateService;
            _renderService = renderService;
            _emailService = emailService;
        }

        [Function(nameof(ProcessEmailNotification))]
        public async Task Run(
            [ServiceBusTrigger("email-notifications", Connection = "ServiceBusConnection")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
#if DEBUG
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            //it is assumed that content type is application/json
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
#endif

            try
            {
                var notification = JsonConvert.DeserializeObject<EmailNotification>(message.Body.ToString());

                if (notification == null)
                {
#if DEBUG
                    _logger.LogInformation("Notification is null");
#endif
                    throw new Exception("Notification is null");
                }

                var clientData = await _clientDataService.GetClientDataAsync(notification.ClientId);
                var template = await _templateService.GetTemplateAsync(notification.ClientId, notification.TemplateId);
                var renderedEmail = await _renderService.RenderTemplateAsync(template, notification.Data);

                var sendResult = await _emailService.SendEmailAsync(notification, clientData.Email, renderedEmail);

                if (sendResult == EmailStatus.Succeeded)
                {
#if DEBUG
                    _logger.LogInformation($"Email sent successfully to {clientData.Email}");
#endif
                    await messageActions.CompleteMessageAsync(message);
                }
                else
                {
#if DEBUG
                    _logger.LogError($"Failed to send email to {clientData.Email}. Status: {sendResult}");
#endif
                    await messageActions.AbandonMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                _logger.LogError(ex, "Error processing email notification");
#endif
                await messageActions.AbandonMessageAsync(message);
            }
        }
    }
}