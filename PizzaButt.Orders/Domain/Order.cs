namespace PizzaButt.Orders.Domain
{
    public record Order
    {
        public Guid Id { get; private set; }
        public string Status { get; private set; }
        public DateTimeOffset CreatedDate { get; private set; }
        public DateTimeOffset UpdatedDate { get; private set; }
        public List<Pizza> Pizzas { get; private set; } = new List<Pizza>();
        public User OrderBy { get; private set; }

        public void Create(Guid id, ICollection<Pizza> pizzas, User orderBy, DateTimeOffset createdDate)
        {
            Id = id;
            Pizzas.AddRange(pizzas);
            OrderBy = orderBy;
            CreatedDate = createdDate;
            Status = OrderStatusEnun.Accepted.ToString();
        }

        public void UpdateStatus(OrderStatusEnun status, DateTimeOffset updatedDate)
        {
            Status = status.ToString();
            UpdatedDate = updatedDate;
        }

        public enum OrderStatusEnun
        {
            Submitted = 0,
            Accepted = 1,
            Shipped = 2,
            Finished = 3,
            Failed = 4,
            Retried = 5,
        }
    }
}
