using Newtonsoft.Json;

namespace My_Microservices.Models
{
    public class Order
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("customerId")]
        public string CustomerId { get; set; } = string.Empty;

        [JsonProperty("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("status")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }

    // POST /orders
    public record CreateOrderRequest(
        string CustomerId,
        string ProductName,
        int Quantity,
        decimal Price
    );

    // PUT /orders/{id}/status
    public record UpdateOrderStatusRequest(
        OrderStatus Status
    );

    // API response — what the client sees
    public record OrderResponse(
        string Id,
        string CustomerId,
        string ProductName,
        int Quantity,
        decimal Price,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    // Mapping extension
    public static class OrderExtensions
    {
        public static OrderResponse ToResponse(this Order order) => new(
            order.Id,
            order.CustomerId,
            order.ProductName,
            order.Quantity,
            order.Price,
            order.Status.ToString(),
            order.CreatedAt,
            order.UpdatedAt
        );

        public static Order ToOrder(this CreateOrderRequest request) => new()
        {
            CustomerId = request.CustomerId,
            ProductName = request.ProductName,
            Quantity = request.Quantity,
            Price = request.Price
        };
    }
}
