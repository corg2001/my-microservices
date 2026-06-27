using Azure.Messaging.ServiceBus;
using My_Microservices.Models;
using System.Text.Json;


namespace My_Microservices.Services
{
    public class ServiceBusService : IServiceBusService, IAsyncDisposable
{
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly ILogger<ServiceBusService> _logger;

        public ServiceBusService(IConfiguration config, ILogger<ServiceBusService> logger)
        {
            _logger = logger;

            var connectionString = config["ServiceBus:ConnectionString"]!;
            var queueName = config["ServiceBus:QueueName"]!;

            _client = new ServiceBusClient(connectionString);
            _sender = _client.CreateSender(queueName);
        }

        public async Task PublishOrderAsync(Order order)
        {
            try
            {
                var orderJson = System.Text.Json.JsonSerializer.Serialize(order, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var message = new ServiceBusMessage(orderJson)
                {
                    MessageId = order.Id,
                    ContentType = "application/json",
                    Subject = "OrderCreated"
                };

                await _sender.SendMessageAsync(message);

                _logger.LogInformation("Order published to Service Bus: {OrderId}", order.Id);
            }
            catch (ServiceBusException ex)
            {
                _logger.LogError(ex, "Service Bus error publishing order: {OrderId} — Reason: {Reason}",
                    order.Id, ex.Reason);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error publishing order: {OrderId}", order.Id);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}