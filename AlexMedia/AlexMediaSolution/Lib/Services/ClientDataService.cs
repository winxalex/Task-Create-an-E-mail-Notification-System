﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using AlexMedia.Interfaces;
using AlexMedia.Models;
using Container = Microsoft.Azure.Cosmos.Container;
using Microsoft.Azure.Cosmos;
using System;
namespace AlexMedia.Services
{
    public class ClientDataService : IClientDataService
    {
        private readonly Container _container;
        private readonly ILogger<ClientDataService> _logger;

        public ClientDataService(Container container, ILogger<ClientDataService> logger)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Upserts (inserts or updates) client data in the Cosmos DB container.
        /// </summary>
        /// <param name="clientData">The client data to be upserted.</param>
        public async Task UpsertClientDataAsync(ClientData clientData)
        {
            // Perform an upsert operation on the container using the client data
            await _container.UpsertItemAsync(clientData, new PartitionKey(clientData.Id));
        }

        /// <summary>
        /// Retrieves client data from the Cosmos DB container based on the client ID.
        /// </summary>
        /// <param name="clientId">The ID of the client to retrieve data for.</param>
        /// <returns>The retrieved ClientData object, or null if not found.</returns>
        public async Task<ClientData> GetClientDataAsync(string clientId)
        {
            try
            {
                // Attempt to read the item from the container using the client ID
                ItemResponse<ClientData> response = await _container.ReadItemAsync<ClientData>(
                    id: clientId,
                    partitionKey: new PartitionKey(clientId)
                );
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Log a warning if the client data is not found (only in DEBUG mode)
#if DEBUG
                _logger.LogWarning(ex, "Client data not found for clientId: {ClientId}", clientId);
#endif
                return null;
            }
        }

        // Other methods...
    }

}