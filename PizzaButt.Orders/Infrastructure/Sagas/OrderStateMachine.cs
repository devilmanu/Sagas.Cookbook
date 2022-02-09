using Automatonymous;
using Kurukuru;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PizzaButt.Contracts;
using System.Drawing;
using Console = Colorful.Console;

namespace PizzaButt.Orders.Infrastructure.Sagas
{
    public class OrderState :
    SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public int CurrentState { get; set; }
    }

    public class OrderStateMachine :
        MassTransitStateMachine<OrderState>
    {

        public OrderStateMachine()
        {
            Event(() => OrderSubmited, x => x.CorrelateById(context => context.Message.Id));
            Event(() => OrderAccepted, x => x.CorrelateById(context => context.Message.Id));
            Event(() => OrderShipped, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => OrderFinished, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => OrderFailed, x => x.CorrelateById(context => context.Message.OrderId));

            InstanceState(x => x.CurrentState);


            Initially(
                When(OrderSubmited)
                    .Then(o => Console.WriteLineFormatted($"Processing in sagas order {o.Data.Id} status {nameof(OrderSubmited)}", Color.YellowGreen))
                    .TransitionTo(Submitted));


            During(Submitted,
                 //Ignore(OrderSubmited), //se raliza esto porque al iniciar el sagas este el primer stado y al ser el primero lo ignoramos
                 When(OrderAccepted)
                    .Then(o => Console.WriteLineFormatted($"Processing in sagas order {o.Data.Id} status {nameof(OrderAccepted)}", Color.LimeGreen))
                    .TransitionTo(Accepted));

            During(Accepted,
                 When(OrderShipped)
                    .Then(o => Console.WriteLineFormatted($"Processing in sagas order {o.Data.OrderId} status {nameof(OrderShipped)}", Color.SeaGreen))
                    .TransitionTo(Shipped));

            During(Shipped,
                 When(OrderFinished)
                    .Then(o => Console.WriteLineFormatted($"Processing  sagas in sagas order {o.Data.OrderId} status {nameof(OrderFinished)}", Color.DarkGreen))
                    .TransitionTo(Finished),
                 When(OrderFailed)
                    .Then(o => Console.WriteLineFormatted($"Processing FAILED in sagas order {o.Data.OrderId} status {nameof(OrderFailed)} reason {o.Data.Error}", Color.Red))
                    .TransitionTo(Failed));

            During(Failed,
                 When(OrderSubmited)
                    .Then(o => Console.WriteLineFormatted($"Processing in sagas order {o.Data.Id} status {nameof(OrderFinished)}", Color.Red))
                    .TransitionTo(Submitted));


            SetCompletedWhenFinalized(); //eliminamos instancia del rpository 
        }


        public Event<OrderSubmitted> OrderSubmited { get; set; }
        public Event<OrderAccepted> OrderAccepted { get; set; }
        public Event<OrderShipped> OrderShipped { get; set; }
        public Event<OrderFinished> OrderFinished { get; set; }
        public Event<OrderFailed> OrderFailed { get; set; }

        public State Submitted { get; set; }
        public State Accepted { get; set; }
        public State Shipped { get; set; }
        public State Finished { get; set; }
        public State Failed { get; set; }
    }


    public class OrderStateMap :
    SagaClassMap<OrderState>
    {
        protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
        {
            entity.Property(x => x.CurrentState).HasMaxLength(64);
            // If using Optimistic concurrency, otherwise remove this property
            //entity.Property(x => x.RowVersion).IsRowVersion();
        }
    }
}
