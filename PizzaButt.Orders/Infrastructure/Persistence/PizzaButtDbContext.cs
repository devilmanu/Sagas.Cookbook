using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PizzaButt.Orders.Domain;
using PizzaButt.Orders.Infrastructure.Sagas;

namespace PizzaButt.Orders.Infrastructure.Persistence
{
    public class PizzaButtDbContext : DbContext
    {
        public PizzaButtDbContext(DbContextOptions<PizzaButtDbContext> options) : base(options)
        {

        }

        public DbSet<Pizza> Pizzas { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Order> Orders { get; set; }
    }


    public class PizzaButtSagasDbContext : SagaDbContext
    {
        public PizzaButtSagasDbContext(DbContextOptions<PizzaButtSagasDbContext> options) : base(options)
        {

        }

        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get { yield return new OrderStateMap(); }
        }
    }

    public class PizzaButtSagasDbContextFactory : IDesignTimeDbContextFactory<PizzaButtSagasDbContext>
    {
        public PizzaButtSagasDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PizzaButtSagasDbContext>();
            optionsBuilder.UseSqlServer("Server=.;Initial Catalog=PizzabuttSagas;User Id=sa;Password=P@ssw0rd;MultipleActiveResultSets=true");
            return new PizzaButtSagasDbContext(optionsBuilder.Options);
        }
    }

    public class PizzaButtDbContextFactory : IDesignTimeDbContextFactory<PizzaButtDbContext>
    {
        public PizzaButtDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PizzaButtDbContext>();
            optionsBuilder.UseSqlServer("Server=.;Initial Catalog=Pizzabutt;User Id=sa;Password=P@ssw0rd;MultipleActiveResultSets=true");
            return new PizzaButtDbContext(optionsBuilder.Options);
        }
    }
}
