using Microsoft.AspNetCore.SignalR;
using PizzaButt.Notifications.Services;

namespace PizzaButt.Notifications.Infrastructure
{
    public class OrdersHub : Hub<IOrders> 
    {
        private readonly IOrderService _orderService;

        public OrdersHub(IOrderService orderService) 
        {
            _orderService = orderService;
        }
        public async Task OrderStatusWasUpdated(Guid id, string status)
        {
            await _orderService.CreateNotification();
            await Clients.All.OrderStatusWasUpdated(id, status);
        }
    }

    public interface IOrders
    {
        Task OrderStatusWasUpdated(Guid id, string status);
    }
}
