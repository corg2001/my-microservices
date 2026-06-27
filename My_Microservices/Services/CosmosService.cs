using Microsoft.Azure.Cosmos;
using My_Microservices.Models;

namespace My_Microservices.Services
{
    public class CosmosService : ICosmosService
    {
        private readonly CosmosClient _client;
        private Container? _container;
        private readonly string _databaseId;
        private readonly string _containerId;
        private readonly string _partitionKeyPath;

        public CosmosService(IConfiguration config)
        {
            var endpoint = config["CosmosDb:Endpoint"]!;
            var authKey = config["CosmosDb:AuthKey"]!;
            _databaseId = config["CosmosDb:DatabaseId"]!;
            _containerId = config["CosmosDb:ContainerId"]!;
            _partitionKeyPath = config["CosmosDb:PartitionKeyPath"]!;

            _client = new CosmosClient(endpoint, authKey, new CosmosClientOptions
            {
                HttpClientFactory = () => new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                }),
                ConnectionMode = ConnectionMode.Gateway,
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });
        }

        public async Task InitializeAsync()
        {
            var database = await _client.CreateDatabaseIfNotExistsAsync(_databaseId);
            _container = (await database.Database.CreateContainerIfNotExistsAsync(
                _containerId,
                _partitionKeyPath
            )).Container;
        }

        // CREATE
        public async Task<Order> SaveOrderAsync(Order order)
        {
            var response = await _container!.CreateItemAsync(order, new PartitionKey(order.Id));
            return response.Resource;
        }

        // READ single
        public async Task<Order?> GetOrderAsync(string id)
        {
            try
            {
                var response = await _container!.ReadItemAsync<Order>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        // READ all (with optional filter by customerId)
        public async Task<List<Order>> GetOrdersAsync(string? customerId = null)
        {
            var query = customerId is null
                ? "SELECT * FROM c"
                : "SELECT * FROM c WHERE c.customerId = @customerId";

            var queryDef = new QueryDefinition(query);
            if (customerId is not null)
                queryDef.WithParameter("@customerId", customerId);

            var results = new List<Order>();
            using var feed = _container!.GetItemQueryIterator<Order>(queryDef);

            while (feed.HasMoreResults)
            {
                var batch = await feed.ReadNextAsync();
                results.AddRange(batch);
            }

            return results;
        }

        // UPDATE status
        public async Task<Order?> UpdateOrderStatusAsync(string id, OrderStatus status)
        {
            var order = await GetOrderAsync(id);
            if (order is null) return null;

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            var response = await _container!.ReplaceItemAsync(order, id, new PartitionKey(id));
            return response.Resource;
        }

        // DELETE
        public async Task<bool> DeleteOrderAsync(string id)
        {
            try
            {
                await _container!.DeleteItemAsync<Order>(id, new PartitionKey(id));
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }
}
