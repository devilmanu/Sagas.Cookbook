namespace PizzaButt.Orders.Services.Oders.Dtos
{
    public class OrderDtoRequest
    {
        public Guid Id { get; set; }
        public string[] Pizzas { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
