using System.ComponentModel.DataAnnotations;

namespace PizzaButt.Orders.Domain
{
    public record User
    {

        public User(string subject, string name)
        {
            Subject = subject;
            Name = name;
        }

        [Key]
        public string Subject { get; private set; }
        public string Name { get; private set; }
    }
}
