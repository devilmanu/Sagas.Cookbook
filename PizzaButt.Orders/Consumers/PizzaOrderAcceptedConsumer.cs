using MassTransit;
using PizzaButt.Contracts;
using PizzaButt.Orders.Services.Oders;
using PizzaButt.Orders.Services.Oders.Dtos;

namespace PizzaButt.Orders.Consumers
{

    class OrderSubmitedConsumer :
    IConsumer<OrderSubmitted>
    {
        ILogger<OrderSubmitedConsumer> _logger;
        private readonly IOrdersService _ordersService;

        public OrderSubmitedConsumer(ILogger<OrderSubmitedConsumer> logger, IOrdersService ordersService)
        {
            _logger = logger;
            _ordersService = ordersService;
        }

        public async Task Consume(ConsumeContext<OrderSubmitted> context)
        {
            _logger.LogInformation($"{nameof(OrderSubmitedConsumer)} OrderId: {context.Message.Id}");
            await _ordersService.AcceptOrderAsync(new OrderDtoRequest
            {
                CreatedAt = DateTime.UtcNow,
                Id = context.Message.Id,
                Pizzas = context.Message.Pizzas
            }, context.CancellationToken);
        }
    }

    class OrderAcceptedConsumer :
    IConsumer<OrderAccepted>
    {
        ILogger<OrderAcceptedConsumer> _logger;
        private readonly IOrdersService _ordersService;

        public OrderAcceptedConsumer(ILogger<OrderAcceptedConsumer> logger, IOrdersService ordersService)
        {
            _logger = logger;
            _ordersService = ordersService;
        }

        public async Task Consume(ConsumeContext<OrderAccepted> context)
        {
            await Task.Delay(30000);
            _logger.LogInformation($"{nameof(OrderAcceptedConsumer)} OrderId: {context.Message.Id}");
            await _ordersService.ShipOrderAsync(new OrderDtoRequest
            {
                CreatedAt = DateTime.UtcNow,
                Id = context.Message.Id,
                Pizzas = context.Message.Pizzas
            }, context.CancellationToken);
        }
    }

    class OrderShippedConsumer :
    IConsumer<OrderShipped>
    {
        ILogger<OrderShippedConsumer> _logger;
        private readonly IOrdersService _ordersService;

        public OrderShippedConsumer(ILogger<OrderShippedConsumer> logger, IOrdersService ordersService)
        {
            _logger = logger;
            _ordersService = ordersService;
        }

        public async Task Consume(ConsumeContext<OrderShipped> context)
        {
            _logger.LogInformation($"{nameof(OrderShippedConsumer)} OrderId: {context.Message.OrderId}");
            await _ordersService.FinishOrderAsync(new OrderDtoRequest
            {
                CreatedAt = DateTime.UtcNow,
                Id = context.Message.OrderId,
                Pizzas = context.Message.Pizzas
            }, context.CancellationToken);
        }
    }

    class OrderFinishedConsumer :
    IConsumer<OrderFinished>
    {
        ILogger<OrderFinishedConsumer> _logger;
        private readonly IOrdersService _ordersService;

        public OrderFinishedConsumer(ILogger<OrderFinishedConsumer> logger, IOrdersService ordersService)
        {
            _logger = logger;
            _ordersService = ordersService;
        }

        public async Task Consume(ConsumeContext<OrderFinished> context)
        {
            _logger.LogInformation($"{nameof(OrderFinishedConsumer)} OrderId: {context.Message.OrderId} DONEEEEEE!!!!!!!!");

        }
    }

    class OrderFailedConsumer :
IConsumer<Fault<OrderFailed>>
    {
        ILogger<OrderFailedConsumer> _logger;

        public OrderFailedConsumer(ILogger<OrderFailedConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderFailed> context)
        {
            //_logger.LogInformation($"{nameof(OrderFailedConsumer)} OrderId: {context.Message.OrderId}");
        }

        public async Task Consume(ConsumeContext<Fault<OrderFailed>> context)
        {
            _logger.LogError($"{nameof(OrderFailedConsumer)} OrderId: {context.Message.Message.OrderId}");
        }
    }



}
