using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using PizzaButt.Contracts;
using PizzaButt.Orders.Infrastructure.Sagas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PizzaButt.Orders.FunctionalTests
{
    public class Sagas
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public IServiceProvider ServiceProvider { get; set; }

        public Sagas(ITestOutputHelper testOutputHelper)
        {
            ServiceProvider = new ServiceCollection()
               .AddMassTransitInMemoryTestHarness(cfg =>
               {
                   cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
                       .InMemoryRepository();
                   cfg.AddSagaStateMachineTestHarness<OrderStateMachine, OrderState>();
               })
               .BuildServiceProvider(true);
            _testOutputHelper = testOutputHelper;
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

                Assert.True(await harness.Published.Any<OrderAccepted>());
                Assert.True(await harness.Consumed.Any<OrderAccepted>());

                Assert.True(await harness.Published.Any<OrderShipped>());
                Assert.True(await harness.Consumed.Any<OrderShipped>());

                var sagaHarness = ServiceProvider.GetRequiredService<IStateMachineSagaTestHarness<OrderState, OrderStateMachine>>();

                Assert.True(await sagaHarness.Consumed.Any<OrderSubmitted>());

                Assert.True(await sagaHarness.Created.Any(x => x.CorrelationId == orderSubmitted.Id));

                var saga = sagaHarness.Created.Contains(orderSubmitted.Id);
                var con = new ConsoleWriter(_testOutputHelper);
                Console.SetOut(con);
                await harness.OutputTimeline(_testOutputHelper.GetType(), opt => opt.Now().IncludeAddress());

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

    public class ConsoleWriter : StringWriter
    {
        private ITestOutputHelper output;
        public ConsoleWriter(ITestOutputHelper output)
        {
            this.output = output;
        }

        public override void WriteLine(string m)
        {
            output.WriteLine(m);
        }
    }
}
