using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using PizzaButt.Contracts;
using PizzaButt.Orders.Infrastructure.Sagas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static PizzaButt.Orders.Domain.Order;

namespace PizzaButt.Orders.FunctionalTests
{
    public class Sagas
    {
        public IServiceProvider ServiceProvider { get; set; }

        public Sagas()
        {
            ServiceProvider = new ServiceCollection()
               .AddMassTransitInMemoryTestHarness(cfg =>
               {
                   cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
                       .InMemoryRepository();
                   cfg.AddSagaStateMachineTestHarness<OrderStateMachine, OrderState>();
               })
               .BuildServiceProvider(true);
        }

        [Fact]
        public async Task Oreder_sagas()
        {
            var harness = ServiceProvider.GetRequiredService<InMemoryTestHarness>();
            await harness.Start();

            try
            {
                var orderSubmitted = new OrderSubmitted
                {
                    Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afc3"),
                    Pizzas = new string[] { "Peperoni", "Piña" },
                    OrderDate = DateTimeOffset.Parse("2022-01-31T15:19:12.175Z")
                };

                await harness.Bus.Publish(orderSubmitted);

                Assert.True(await harness.Published.Any<OrderSubmitted>());

                Assert.True(await harness.Consumed.Any<OrderSubmitted>());

                var sagaHarness = ServiceProvider.GetRequiredService<IStateMachineSagaTestHarness<OrderState, OrderStateMachine>>();

                Assert.True(await sagaHarness.Consumed.Any<OrderSubmitted>());

                Assert.True(await sagaHarness.Created.Any(x => x.CorrelationId == orderSubmitted.Id));

                var saga = sagaHarness.Created.Contains(orderSubmitted.Id);
                Assert.True(saga != null);
                Assert.True(saga.CurrentState == 3); 
            }
            finally
            {
                await harness.Stop();

                harness.Dispose();
            }
        }
    }
}
