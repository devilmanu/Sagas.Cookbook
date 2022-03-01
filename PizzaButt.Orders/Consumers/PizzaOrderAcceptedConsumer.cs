using MassTransit;
using PizzaButt.Contracts;
using PizzaButt.Orders.Services.Oders;
using PizzaButt.Orders.Services.Oders.Dtos;

namespace PizzaButt.Orders.Consumers
{


    public class MertricsDto
    {
        public Guid Id { get; set; }
        public string Source { get; set; }
        public DateTimeOffset DateUtc { get; set; }

    }

    class OrderSubmitedConsumer :
    IConsumer<OrderSubmitted>
    {
        ILogger<OrderSubmitedConsumer> _logger;
        private readonly IOrdersService _ordersService;
        public readonly HttpClient HttpClient;

        public OrderSubmitedConsumer(ILogger<OrderSubmitedConsumer> logger, IOrdersService ordersService, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _ordersService = ordersService;
            HttpClient = clientFactory.CreateClient("PizzaButt.Metrics");

        }

        public async Task Consume(ConsumeContext<OrderSubmitted> context)
        {
            _logger.LogInformation($"{nameof(OrderSubmitedConsumer)} OrderId: {context.Message.Id} retry count {context.GetRetryCount()} {context.GetRetryAttempt()}");
            await _ordersService.AcceptOrderAsync(new OrderDtoRequest
            {
                CreatedAt = DateTime.UtcNow,
                Id = context.Message.Id,
                Pizzas = context.Message.Pizzas,
                ThrowError = context.Message.ThrowError
            }, context.CancellationToken);
            await HttpClient.PostAsJsonAsync("/metrics", new MertricsDto
            {
                DateUtc = DateTime.UtcNow,
                Id = context.Message.Id,
                Source = nameof(OrderSubmitted)
            });
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
            _logger.LogInformation($"{nameof(OrderAcceptedConsumer)} OrderId: {context.Message.Id} retry count {context.GetRetryCount()} {context.GetRetryAttempt()}");
            await _ordersService.ShipOrderAsync(new OrderDtoRequest
            {
                CreatedAt = DateTime.UtcNow,
                Id = context.Message.Id,
                Pizzas = context.Message.Pizzas,
                ThrowError = context.Message.ThrowError
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
            _logger.LogInformation($"{nameof(OrderShippedConsumer)} OrderId: {context.Message.OrderId} retry count {context.GetRetryCount()} {context.GetRetryAttempt()}");
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

    class OrderRetriedConsumer :
        IConsumer<OrderRetried>
    {
        ILogger<OrderRetriedConsumer> _logger;
        private readonly IOrdersService _ordersService;

        public OrderRetriedConsumer(ILogger<OrderRetriedConsumer> logger, IOrdersService ordersService)
        {
            _logger = logger;
            _ordersService = ordersService;
        }

        public async Task Consume(ConsumeContext<OrderRetried> context)
        {
            _logger.LogInformation($"{nameof(OrderRetriedConsumer)} OrderId: {context.Message.OrderId} DONEEEEEE!!!!!!!!");

        }
    }

    class OrderFailedConsumer :
        IConsumer<Fault<OrderSubmitted>>,
        IConsumer<Fault<OrderAccepted>>,
        IConsumer<Fault<OrderShipped>>,
        IConsumer<Fault<OrderFinished>>
    {
        ILogger<OrderFailedConsumer> _logger;
        private readonly IOrdersService _ordersService;

        public OrderFailedConsumer(ILogger<OrderFailedConsumer> logger, IOrdersService ordersService)
        {
            _logger = logger;
            _ordersService = ordersService;
        }

        public async Task Consume(ConsumeContext<Fault<OrderFinished>> context)
        {
            _logger.LogError($"{nameof(OrderFailedConsumer)} OrderId: {context.Message.Message.OrderId} retry count {context.GetRetryAttempt()}");
            await _ordersService.FailedOrderAsync(new OrderDtoRequest
            {
                CreatedAt = DateTime.UtcNow,
                Id = context.Message.Message.OrderId,
                Pizzas = context.Message.Message.Pizzas
            }, context.CancellationToken);

        }

        public async Task Consume(ConsumeContext<Fault<OrderShipped>> context)
        {
            _logger.LogError($"{nameof(OrderFailedConsumer)} OrderId: {context.Message.Message.OrderId} retry count {context.GetRetryAttempt()}");
            await _ordersService.FailedOrderAsync(new OrderDtoRequest
            {
                CreatedAt = DateTime.UtcNow,
                Id = context.Message.Message.OrderId,
                Pizzas = context.Message.Message.Pizzas
            }, context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<Fault<OrderAccepted>> context)
        {
            _logger.LogError($"{nameof(OrderFailedConsumer)} OrderId: {context.Message.Message.Id}, retry count {context.GetRetryCount()} {context.GetRetryAttempt()}");
            await _ordersService.FailedOrderAsync(new OrderDtoRequest
            {
                CreatedAt = DateTime.UtcNow,
                Id = context.Message.Message.Id,
                Pizzas = context.Message.Message.Pizzas,
            }, context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<Fault<OrderSubmitted>> context)
        {
            _logger.LogError($"{nameof(OrderFailedConsumer)} OrderId: {context.Message.Message.Id} retry count {context.GetRetryCount()} {context.GetRetryAttempt()}");
            await _ordersService.FailedOrderAsync(new OrderDtoRequest
            {
                CreatedAt = DateTime.UtcNow,
                Id = context.Message.Message.Id,
                Pizzas = context.Message.Message.Pizzas
            }, context.CancellationToken);
        }
    }



}
