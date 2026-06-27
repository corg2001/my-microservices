using Dapr;                              // For Dapr attributes
using Dapr.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using My_Microservices.Models;
using My_Microservices.Services;

namespace My_Microservices.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly DaprClient _daprClient;
        private readonly ICosmosService _cosmosService;
        //private readonly ServiceBusService _serviceBus;

        public OrderController(DaprClient daprClient, ICosmosService cosmos)
        {
            _cosmosService = cosmos;
            _daprClient = daprClient;
        }

        // POST: Create Order + Save to State Store + Publish Event
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            // Save to Dapr State Store
            await _daprClient.SaveStateAsync("statestore", order.Id, order);

            // Also save to Cosmos DB
            await _cosmosService.SaveOrderAsync(order);

            // Publish event using Dapr Pub/Sub
            await _daprClient.PublishEventAsync("pubsub", "orders", order);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        // GET: Retrieve Order from Dapr State Store
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(string id)
        {
            var order = await _daprClient.GetStateAsync<Order>("statestore", id);

            if (order == null)
            {
                // Fallback to Cosmos DB
                order = await _cosmosService.GetOrderAsync(id);
            }

            return order != null ? Ok(order) : NotFound();
        }

        // Bonus: Health Check
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", service = "OrderService" });
        }
    }
}
