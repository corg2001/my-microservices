using My_Microservices.Models;

namespace My_Microservices.Services
{
    public interface ICosmosService
    {
        /// <summary>
        /// Creates the database and container if they don't exist.
        /// Call once at startup before accepting requests.
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Creates a new order document in Cosmos DB.
        /// </summary>
        Task<Order> SaveOrderAsync(Order order);

        /// <summary>
        /// Retrieves a single order by id.
        /// Returns null if not found.
        /// </summary>
        Task<Order?> GetOrderAsync(string id);

        /// <summary>
        /// Retrieves all orders. Optionally filter by customerId.
        /// </summary>
        Task<List<Order>> GetOrdersAsync(string? customerId = null);

        /// <summary>
        /// Updates the status of an existing order.
        /// Returns null if order not found.
        /// </summary>
        Task<Order?> UpdateOrderStatusAsync(string id, OrderStatus status);

        /// <summary>
        /// Deletes an order by id.
        /// Returns false if not found.
        /// </summary>
        Task<bool> DeleteOrderAsync(string id);
    }
}
