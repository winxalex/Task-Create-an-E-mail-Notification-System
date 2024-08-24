using WINX.Models;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using WINX.Interfaces;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using System;
using Microsoft.Extensions.Logging;

namespace WINX.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<TemplateService> _logger;

        public TemplateService(IConfiguration configuration, ILogger<TemplateService> logger)
        {
            _logger = logger;
            var connectionString = configuration["AzureStorage:ConnectionString"];
            var containerName = configuration["AzureStorage:TemplateContainerName"];

            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        }


        //The code uses a naming convention ({clientId}/{templateName}) which creates a hierarchical structure, making it easy to organize templates by client.

        /// <summary>
        /// Saves an email template to Azure Blob Storage.
        /// </summary>
        /// <param name="template">The email template to save.</param>
        /// <returns>The saved email template with updated Id.</returns>
        public async Task<EmailTemplate> SaveTemplateAsync(EmailTemplate template)
        {
            try
            {
                // Create a unique blob name using client ID and template details
                string blobName = $"{template.ClientId}/{template.Id}_{template.Name}";

                // Get a reference to the blob
                var blobClient = _containerClient.GetBlobClient(blobName);

                // Convert template content to a stream
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(template.Content));

                // Upload the stream to the blob, overwriting if it exists
                await blobClient.UploadAsync(stream, true);

                // Update the template ID with the full blob name
                template.Id = blobName;
                return template;
            }
            catch (Exception ex)
            {
#if DEBUG
                // Log the error in debug mode
                _logger.LogError(ex, "Error saving template for client {ClientId}", template.ClientId);
#endif
                throw;
            }
        }

        /// <summary>
        /// Retrieves all email templates for a specific client.
        /// </summary>
        /// <param name="clientId">The ID of the client.</param>
        /// <returns>A list of email templates for the specified client.</returns>
        public async Task<List<EmailTemplate>> GetClientTemplatesAsync(string clientId)
        {
            try
            {
                // Validate client ID
                if (string.IsNullOrEmpty(clientId))
                {
                    throw new ArgumentException("Client ID cannot be null or empty");
                }

                var templates = new List<EmailTemplate>();
                // Iterate through blobs with the client ID prefix
                await foreach (var blob in _containerClient.GetBlobsAsync(prefix: $"{clientId}/"))
                {
                    // Get blob client for each matching blob
                    var blobClient = _containerClient.GetBlobClient(blob.Name);
                    // Download blob content
                    var content = await blobClient.DownloadContentAsync();
                    // Extract template name from blob name
                    var nameParts = blob.Name.Substring(blob.Name.LastIndexOf('/') + 1).Split('_');
                    // Create and add EmailTemplate object to the list
                    templates.Add(new EmailTemplate
                    {
                        Id = nameParts[0],
                        Name = nameParts[1],
                        Content = content.Value.Content.ToString(),
                        ClientId = clientId
                    });
                }
                return templates;
            }
            catch (Exception ex)
            {
#if DEBUG
                _logger.LogError(ex, "Error retrieving templates for client {ClientId}", clientId);
#endif
                throw;
            }
        }

        /// <summary>
        /// Retrieves all email templates for all clients.
        /// </summary>
        /// <returns>A list of all email templates.</returns>
        public async Task<List<EmailTemplate>> GetAllTemplatesAsync()
        {
            try
            {
                var templates = new List<EmailTemplate>();
                // Iterate through all blobs in the container
                await foreach (var blob in _containerClient.GetBlobsAsync())
                {
                    // Get blob client for each blob
                    var blobClient = _containerClient.GetBlobClient(blob.Name);
                    // Download blob content
                    var content = await blobClient.DownloadContentAsync();
                    // Split blob name to extract client ID and template name
                    var parts = blob.Name.Split('/');
                    // Create and add EmailTemplate object to the list
                    templates.Add(new EmailTemplate
                    {
                        Id = blob.Name,
                        Name = parts.Length > 1 ? parts[1] : blob.Name,
                        Content = content.Value.Content.ToString(),
                        ClientId = parts[0]
                    });
                }
                return templates;
            }
            catch (Exception ex)
            {
#if DEBUG
                _logger.LogError(ex, "Error retrieving all templates");
#endif
                throw;
            }
        }

        /// <summary>
        /// Deletes an email template from Azure Blob Storage.
        /// </summary>
        /// <param name="clientId">The ID of the client.</param>
        /// <param name="templateId">The ID of the template to delete.</param>
        public async Task DeleteTemplateAsync(string clientId, string templateId)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(templateId))
                {
                    throw new ArgumentException("Client ID and Template ID cannot be null or empty");
                }

                // Get blobs matching the client ID and template ID prefix
                var blobsToDelete = _containerClient.GetBlobsAsync(prefix: $"{clientId}/{templateId}_");
                await foreach (var blob in blobsToDelete)
                {
                    // Get blob client for each matching blob
                    var blobClient = _containerClient.GetBlobClient(blob.Name);
                    // Delete the blob if it exists
                    await blobClient.DeleteIfExistsAsync();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                _logger.LogError(ex, "Error deleting template {TemplateId} for client {ClientId}", templateId, clientId);
#endif
                throw;
            }
        }


        /// <summary>
        /// Retrieves the content of a specific email template for a client.
        /// </summary>
        /// <param name="clientId">The ID of the client.</param>
        /// <param name="templateId">The ID of the template.</param>
        /// <returns>The content of the email template, or null if not found.</returns>
        public async Task<string> GetTemplateAsync(string clientId, string templateId)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(templateId))
                {
                    throw new ArgumentException("Client ID and template ID cannot be null or empty");
                }

                // Get blobs matching the client ID and template ID prefix
                var blobsToSearch = _containerClient.GetBlobsAsync(prefix: $"{clientId}/{templateId}_");
                await foreach (var blob in blobsToSearch)
                {
                    // Get blob client for the matching blob
                    var blobClient = _containerClient.GetBlobClient(blob.Name);
                    // Download blob content
                    var response = await blobClient.DownloadContentAsync();
                    // Return the content as a string
                    return response.Value.Content.ToString();
                }

                // Throw exception if template is not found
                throw new FileNotFoundException($"Template with ID {templateId} not found for client {clientId}");
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
#if DEBUG
                _logger.LogError(ex, "Template not found with ID {TemplateId} for client {ClientId}", templateId, clientId);
#endif
                throw;
            }
            catch (Exception ex)
            {
#if DEBUG
                _logger.LogError(ex, "Error retrieving template with ID {TemplateId} for client {ClientId}", templateId, clientId);
#endif
                throw;
            }
        }

        // Other methods...
    }
}