using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using AlexMedia.Models;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using WINX.Extensions; // Add this using directive

namespace AlexMedia.Services
{
    public class CampaignService
    {
        private readonly ServiceBusSender _sender;
        private readonly string _serviceBusQueueName;
        private readonly ILogger<CampaignService> _logger; // Add logger field

        public CampaignService(IConfiguration configuration, ServiceBusClient serviceBusClient, ILogger<CampaignService> logger) // Inject logger
        {
            _serviceBusQueueName = configuration["ServiceBus:QueueName"];
            _sender = serviceBusClient.CreateSender(_serviceBusQueueName);
            _logger = logger; // Assign logger
        }

        public async Task StartCampaignAsync(string subject, string sender, Stream xmlFileStream)
        {
            /// <summary>
            /// Initiates the campaign by processing the provided XML file stream and sending notifications.
            /// </summary>
            await ProcessXmlAndSendNotificationsAsync(subject, sender, xmlFileStream);
        }


        /// <summary>
        /// Processes the XML file and sends notifications for each client.
        /// </summary>
        /// <param name="subject">The email subject.</param>
        /// <param name="senderEmail">The sender's email address.</param>
        /// <param name="xmlStream">The XML stream containing client data.</param>
        /// <remarks>
        /// The XML structure should be as follows:
        /// <code>
        /// <Clients>
        ///     <Client ID="12345">
        ///         <Template Id="1">
        ///             <Name>TemplateName.html</Name>
        ///             <MarketingData>{json data string representation}</MarketingData>
        ///         </Template>
        ///     </Client>
        ///     <Client ID="54321">
        ///         <Template Id="2">
        ///             <Name>TemplateName2.html</Name>
        ///             <MarketingData>{json data string representation}</MarketingData>
        ///         </Template>
        ///     </Client>
        ///     <!-- More clients -->
        /// </Clients>
        /// </code>
        /// </remarks>
        private async Task ProcessXmlAndSendNotificationsAsync(string subject, string senderEmail, Stream xmlStream)
        {
            using var reader = XmlReader.Create(xmlStream, new XmlReaderSettings { Async = true });

            if (await reader.ReadToDescendantAsync("Client"))
            {
                var clientId = reader.GetAttribute("ID"); // Corrected attribute name to "ID"
                if (!string.IsNullOrEmpty(clientId))
                {
                    try
                    {
                        if (await reader.ReadToDescendantAsync("Template"))
                        {

                            var templateId = reader.GetAttribute("Id");
                            var name = string.Empty; // Initialize name
                            if (await reader.ReadToDescendantAsync("Name"))
                            {

                                name = await reader.ReadElementContentAsStringAsync();

                            }

                            var marketingData = string.Empty; // Initialize marketingData
                            if (await reader.ReadToDescendantAsync("MarketingData")) // Use extension method
                            {
                                marketingData = await reader.ReadElementContentAsStringAsync();

                            }

                            var notification = new EmailNotification
                            {
                                ClientId = clientId,
                                SenderEmail = senderEmail,
                                Data = marketingData,
                                TemplateId = templateId,
                                Subject = subject
                            };

                            var message = new ServiceBusMessage(JsonConvert.SerializeObject(notification));
                            await _sender.SendMessageAsync(message); // Send email for each client



                        }
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        _logger.LogError($"Error processing client {clientId}: {ex.Message}"); // Log error using logger
#endif
                    }
                }
            }
        }
    }
}


