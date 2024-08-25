using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using AlexMedia.Models;
using Azure.Messaging.ServiceBus;

namespace AlexMedia.Services
{
    public class CampaignService
    {
        private readonly ServiceBusSender _sender;
        private readonly string _serviceBusQueueName;

        public CampaignService(IConfiguration configuration, ServiceBusClient serviceBusClient)
        {
            _serviceBusQueueName = configuration["ServiceBus:QueueName"];
            _sender = serviceBusClient.CreateSender(_serviceBusQueueName);
        }

        public async Task StartCampaignAsync(string subject, string sender, Stream xmlFileStream)
        {
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
            // Create an XML reader to parse the input stream
            using var reader = XmlReader.Create(xmlStream, new XmlReaderSettings { Async = true });

            while (await reader.ReadAsync())
            {
                // Check if the current node is a "Client" element
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Client")
                {
                    // Get the client ID from the "Id" attribute
                    var clientId = reader.GetAttribute("Id");
                    if (!string.IsNullOrEmpty(clientId))
                    {
                        // Create a new EmailNotification object with the parsed data
                        var notification = new EmailNotification
                        {
                            ClientId = clientId,
                            SenderEmail = senderEmail,
                            Data = reader.GetAttribute("MarketingData"),
                            TemplateId = reader.GetAttribute("TemplateId"),
                            Subject = subject
                        };

                        // Serialize the notification object to JSON
                        var message = new ServiceBusMessage(JsonConvert.SerializeObject(notification));

                        // Send the message to the Service Bus queue
                        await _sender.SendMessageAsync(message);
                    }
                }
            }
        }
    }
}