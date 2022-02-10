using System;
using System.Collections.Generic;
using System.Text;

namespace PizzaButt.Contracts
{
    public class OrderSubmitted
    {
        public Guid Id { get; set; }
        public string[] Pizzas { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public bool ThrowError { get; set; }
    }

    public class OrderAccepted
    {
        public Guid Id { get; set; }
        public string[] Pizzas { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public bool ThrowError { get; set; }
    }

    public class OrderShipped
    {
        public Guid OrderId { get; set; }
        public string[] Pizzas { get; set; }
        public DateTimeOffset OrderDate { get; set; }
    }


    public class OrderFinished

    {
        public Guid OrderId { get; set; }
        public string[] Pizzas { get; set; }
        public DateTimeOffset OrderDate { get; set; }
    }

    public class OrderRetried

    {
        public Guid OrderId { get; set; }
        public string[] Pizzas { get; set; }
        public DateTimeOffset OrderDate { get; set; }
    }

    public class OrderFailed
    {
        public Guid OrderId { get; set; }
        public string Error { get; set; }
        public DateTimeOffset OrderDate { get; set; }
    }
}
