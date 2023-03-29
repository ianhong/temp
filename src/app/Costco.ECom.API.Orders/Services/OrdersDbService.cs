﻿using AlwaysOn.Shared.TelemetryExtensions;
using Azure.Core;

namespace Costco.ECom.API.Orders.Services
{
    public class OrdersDbService : CosmosDbService, IOrdersService

    {
        private readonly CosmosLinqSerializerOptions _cosmosSerializationOptions = new CosmosLinqSerializerOptions() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase };

        public OrdersDbService(ILogger<CosmosDbService> logger,
            SysConfiguration sysConfig,
            TelemetryClient tc,
            TokenCredential token,
            AppInsightsCosmosRequestHandler appInsightsRequestHandler) : base(logger, sysConfig, tc, token, appInsightsRequestHandler)
        {
            logger.LogInformation("Initializing Cosmos DB client with endpoint {endpoint} in ApplicationRegion {azureRegion}. Database name {databaseName}", sysConfig.CosmosEndpointUri, sysConfig.AzureRegion, sysConfig.CosmosDBDatabaseName);
        }

        public async Task AddNewOrderItemAsync(OrderItem item)
        {
            var startTime = DateTime.UtcNow;
            ItemResponse<OrderItem> response = null;
            CosmosDiagnostics diagnostics = null;
            var success = false;
            var conflict = false;
            try
            {
                response = await _ordersContainer.CreateItemAsync(item, new PartitionKey(item.Id.ToString()));
                diagnostics = response.Diagnostics;
                success = true;
            }
            catch (CosmosException cex) when (cex.StatusCode == HttpStatusCode.Conflict)
            {
                diagnostics = cex.Diagnostics;
                _logger.LogWarning("OrderItem with id {OrderItemId} already exists. Ignoring item", item.Id);
                conflict = true;
                success = true;
            }
            catch (CosmosException cex)
            {
                diagnostics = cex.Diagnostics;
                throw new AlwaysOnDependencyException(cex.StatusCode, innerException: cex);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unknown exception on request to Cosmos DB");
                throw new AlwaysOnDependencyException(HttpStatusCode.InternalServerError, "Unknown exception on request to Cosmos DB", innerException: e);
            }
            finally
            {
                var overallDuration = DateTime.UtcNow - startTime;
                var telemetry = new DependencyTelemetry()
                {
                    Type = AppInsightsDependencyType,
                    Data = $"OrderItemId={item.Id}, Partitionkey={item.Id}",
                    Name = "Add OrderItem",
                    Timestamp = startTime,
                    Duration = diagnostics != null ? diagnostics.GetClientElapsedTime() : overallDuration,
                    Target = diagnostics != null ? diagnostics.GetContactedRegions().FirstOrDefault().uri?.Host : _dbClient.Endpoint.Host,
                    Success = success
                };
                if (response != null)
                    telemetry.Metrics.Add("CosmosDbRequestUnits", response.RequestCharge);

                if (conflict)
                {
                    telemetry.Properties.Add("ConflictOnInsert", conflict.ToString());
                }
                _telemetryClient.TrackDependency(telemetry);
            }
        }

        public async Task<OrderItem> GetOrderItemByIdAsync(Guid itemId)
        {
            string partitionKey = itemId.ToString();
            var startTime = DateTime.UtcNow;
            FeedResponse<OrderItem> responseMessage = null;
            CosmosDiagnostics diagnostics = null;
            var success = false;
            try
            {
                // Read the item as a stream for higher performance.
                // See: https://github.com/Azure/azure-cosmos-dotnet-v3/blob/master/Exceptions.md#stream-api
                var queryDefintion = new QueryDefinition("SELECT * FROM c WHERE c.id = @itemId")
                                                        .WithParameter("@itemId", itemId);
                var query = _inventoryContainer.GetItemQueryIterator<OrderItem>(queryDefintion);
                responseMessage = await query.ReadNextAsync();


                // Item stream operations do not throw exceptions for better performance
                if (responseMessage.Count > 0)
                {
                    diagnostics = responseMessage.Diagnostics;
                    success = true;
                    return responseMessage?.FirstOrDefault();
                }
                else if (responseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    // No OrderItem found for the id/partitionkey
                    success = true;
                    return null;
                }
                else
                {
                    throw new AlwaysOnDependencyException(responseMessage.StatusCode, $"Unexpected status code in {nameof(GetOrderItemByIdAsync)}. Code={responseMessage.StatusCode}");
                }
            }
            catch (CosmosException cex)
            {
                diagnostics = cex.Diagnostics;
                throw new AlwaysOnDependencyException(cex.StatusCode, innerException: cex);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unknown exception on request to Cosmos DB");
                throw new AlwaysOnDependencyException(HttpStatusCode.InternalServerError, "Unknown exception on request to Cosmos DB", innerException: e);
            }
            finally
            {
                var overallDuration = DateTime.UtcNow - startTime;
                var telemetry = new DependencyTelemetry()
                {
                    Type = AppInsightsDependencyType,
                    Data = $"OrderItemId={itemId}, Partitionkey={partitionKey}",
                    Name = "Get OrderItem by Id",
                    Timestamp = startTime,
                    Duration = diagnostics != null ? diagnostics.GetClientElapsedTime() : overallDuration,
                    Target = diagnostics != null ? diagnostics.GetContactedRegions().FirstOrDefault().uri?.Host : _dbClient.Endpoint.Host,
                    Success = success
                };
                if (responseMessage != null)
                {
                    telemetry.Metrics.Add("CosmosDbRequestUnits", responseMessage.Headers.RequestCharge);
                }

                _telemetryClient.TrackDependency(telemetry);
            }
        }

        public async Task<IEnumerable<OrderItem>> ListOrderItemsAsync(int limit)
        {
            var queryable = _inventoryContainer.GetItemLinqQueryable<OrderItem>(linqSerializerOptions: _cosmosSerializationOptions)
                 .Take(limit);
            var result = await ListDocumentsByQueryAsync<OrderItem>(queryable);
            return result;
        }

        public Task UpsertOrderItemAsync(OrderItem item)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<OrderItem>> ListInventoryAsync(int limit)
        {
            var queryable = _inventoryContainer.GetItemLinqQueryable<OrderItem>(linqSerializerOptions: _cosmosSerializationOptions)
                .Take(limit);
            var result = await ListDocumentsByQueryAsync<OrderItem>(queryable);
            return result;
        }

    }
}