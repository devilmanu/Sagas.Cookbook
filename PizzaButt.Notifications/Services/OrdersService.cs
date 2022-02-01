namespace PizzaButt.Notifications.Services
{
    public class OrderService : IOrderService
    {

        public Task CreateNotification()
        {
            //Save In database
            return Task.CompletedTask;
        }
    }
}
