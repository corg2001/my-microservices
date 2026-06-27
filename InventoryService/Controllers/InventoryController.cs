using Dapr;
using Microsoft.AspNetCore.Mvc;
using InventoryService.Models;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(ILogger<InventoryController> logger)
        {
            _logger = logger;
        }

        [Topic("pubsub", "order-created")]
        [HttpPost("order-created")]
        public async Task<IActionResult> ProcessOrder([FromBody] Order order)
        {
            _logger.LogInformation("📦 InventoryService received order: {OrderId} for {CustomerId}",
                order.Id, order.CustomerId);

            // Simulate inventory update
            await Task.Delay(500);

            _logger.LogInformation("✅ Inventory updated for order {OrderId} — {Quantity}x {ProductName}",
                order.Id, order.Quantity, order.ProductName);

            return Ok(new
            {
                message = "Inventory updated successfully",
                orderId = order.Id,
                productName = order.ProductName,
                quantity = order.Quantity
            });
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", service = "InventoryService" });
        }
    }
}