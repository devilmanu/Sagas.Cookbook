using System.ComponentModel.DataAnnotations;

namespace PizzaButt.Orders.Domain
{
    public record Pizza
    {
        public Pizza(string type) 
        {
            Type = type;
        }
        [Key]
        public string Type { get; private set; }
    }
}
