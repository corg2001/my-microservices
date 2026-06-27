using My_Microservices.Models;

namespace My_Microservices.Services
{
    public interface IServiceBusService
    {
        Task PublishOrderAsync(Order order);
    }
}
