namespace PizzaButt.Mdetrics.Controllers
{
    public class MetricsDto
    {
        public Guid Id { get; set; }
        public string Source { get; set; }
        public DateTimeOffset DateUtc { get; set; }

    }
}