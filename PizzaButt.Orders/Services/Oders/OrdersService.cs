using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaButt.Contracts;
using PizzaButt.Orders.Domain;
using PizzaButt.Orders.Infrastructure.Persistence;
using PizzaButt.Orders.Services.Oders.Dtos;
using System.Linq.Expressions;
using static PizzaButt.Orders.Domain.Order;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PizzaButt.Orders.Services.Oders
{
    public class OrdersService : IOrdersService
    {


        private readonly PizzaButtDbContext _pizzaButtDbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IConfiguration _configuration;

        public IHttpContextAccessor _httpContextAccessor { get; }

        public OrdersService(PizzaButtDbContext pizzaButtDbContext, IHttpContextAccessor httpContextAccessor, IPublishEndpoint publishEndpoint, IConfiguration configuration)
        {
            _pizzaButtDbContext = pizzaButtDbContext;
            _httpContextAccessor = httpContextAccessor;
            _publishEndpoint = publishEndpoint;
            _configuration = configuration;
        }

        private Task UseDelayAsync()
        {
            if (_configuration.GetSection("Sagas:DelayBetweenSteps").Exists())
            {
                return Task.Delay((_configuration.GetSection("Sagas:DelayBetweenSteps").Get<int>()));
            }
            return Task.CompletedTask;
        }
        private string RandomString(int length = 1)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<IEnumerable<OrderDtoResponse>> GetOrdersAsync(CancellationToken cancellationToken)
        {
            var response = await _pizzaButtDbContext.Orders
                .Include(o => o.Pizzas)
                .Include(o => o.OrderBy)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return response.Select(o => new OrderDtoResponse
            {
                Id = o.Id,
                Status = o.Status
            });
        }

        public async Task<OrderDtoResponse> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var response = await _pizzaButtDbContext.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
            if (response == null)
                throw new KeyNotFoundException();

            return new OrderDtoResponse
            {
                Id = response.Id,
                Status = response.Status,
            };
        }

        public async Task SubmmitOrderAsync(OrderDtoRequest request, CancellationToken cancellationToken)
        {
            if (_pizzaButtDbContext.Orders.Any(o => o.Id == request.Id))
                throw new ResourceFoundException();

            var user = _pizzaButtDbContext.Users.FirstOrDefault(o => o.Subject == request.UserId) ?? new User(request.UserId, request.UserName);
            var pizzasRequest = request.Pizzas.Select(o => new Pizza(o)).ToHashSet();

            var pizzasExisting = _pizzaButtDbContext.Pizzas.Where(p => pizzasRequest.Contains(p)).ToList();
            if (pizzasExisting == null)
                await _pizzaButtDbContext.AddRangeAsync(pizzasRequest, cancellationToken);

            var order = new Order();
            order.Create(request.Id, pizzasExisting.Intersect(pizzasRequest).ToList(), user, request.CreatedAt);
            await _pizzaButtDbContext.Orders.AddAsync(order, cancellationToken);
            await _pizzaButtDbContext.SaveChangesAsync(cancellationToken);
            await _publishEndpoint.Publish(new OrderSubmitted
            {
                Id = request.Id,
                OrderDate = request.CreatedAt,
                Pizzas = request.Pizzas,
                ThrowError = request.ThrowError
            }, cancellationToken);
        }

        public async Task AcceptOrderAsync(OrderDtoRequest request, CancellationToken cancellationToken)
        {
            await UseDelayAsync();
            var order = await _pizzaButtDbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            if (order == null)
                throw new KeyNotFoundException();
            order.UpdateStatus(OrderStatusEnun.Accepted, request.CreatedAt);
            _pizzaButtDbContext.Update(order);
            await _pizzaButtDbContext.SaveChangesAsync(cancellationToken);
            await _publishEndpoint.Publish(new OrderAccepted
            {
                Id = order.Id,
                OrderDate = request.CreatedAt,
                Pizzas = order.Pizzas.Select(o => o.Type).ToArray(),
                ThrowError = request.ThrowError
            });
        }

        public async Task ShipOrderAsync(OrderDtoRequest request, CancellationToken cancellationToken)
        {
            await UseDelayAsync();
            if (request.ThrowError)
                throw new KeyNotFoundException("Exception was generated by user");

            var order = await _pizzaButtDbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            if (order == null)
                throw new KeyNotFoundException();
            order.UpdateStatus(OrderStatusEnun.Shipped, request.CreatedAt);
            _pizzaButtDbContext.Update(order);
            await _pizzaButtDbContext.SaveChangesAsync(cancellationToken);
            await _publishEndpoint.Publish(new OrderShipped
            {
                OrderId = order.Id,
                OrderDate = request.CreatedAt,
                Pizzas = order.Pizzas.Select(o => o.Type).ToArray()
            });
        }

        public async Task FinishOrderAsync(OrderDtoRequest request, CancellationToken cancellationToken)
        {
            await UseDelayAsync();
            var order = await _pizzaButtDbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            if (order == null)
                throw new KeyNotFoundException();
            order.UpdateStatus(OrderStatusEnun.Finished, request.CreatedAt);
            _pizzaButtDbContext.Update(order);
            await _pizzaButtDbContext.SaveChangesAsync(cancellationToken);
            await _publishEndpoint.Publish(new OrderFinished
            {
                OrderId = order.Id,
                OrderDate = request.CreatedAt,
                Pizzas = order.Pizzas.Select(o => o.Type).ToArray()
            });
        }

        public async Task RetriedOrderAsync(OrderDtoRequest request, CancellationToken cancellationToken)
        {
            await UseDelayAsync();
            var order = await _pizzaButtDbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            if (order == null)
                throw new KeyNotFoundException();
            order.UpdateStatus(OrderStatusEnun.Retried, request.CreatedAt);
            _pizzaButtDbContext.Update(order);
            await _pizzaButtDbContext.SaveChangesAsync(cancellationToken);
            await _publishEndpoint.Publish(new OrderRetried
            {
                OrderId = order.Id,
                OrderDate = request.CreatedAt,
                Pizzas = order.Pizzas.Select(o => o.Type).ToArray()
            });
        }


        public async Task FailedOrderAsync(OrderDtoRequest request, CancellationToken cancellationToken)
        {
            await UseDelayAsync();
            var order = await _pizzaButtDbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            if (order == null)
                throw new KeyNotFoundException();
            order.UpdateStatus(OrderStatusEnun.Failed, request.CreatedAt);
            _pizzaButtDbContext.Update(order);
            await _pizzaButtDbContext.SaveChangesAsync(cancellationToken);
            await _publishEndpoint.Publish(new OrderFailed
            {
                OrderId = order.Id,
                OrderDate = request.CreatedAt,
                Error = "?¿?¿¿?¿?¿"
            });
        }


        public async Task DeleteAllAsync(CancellationToken cancellationToken)
        {
            var  orders = await _pizzaButtDbContext.Orders.ToListAsync();
            _pizzaButtDbContext.Orders.RemoveRange(orders);
            await _pizzaButtDbContext.SaveChangesAsync();
        }
    }

    public static class DbSetExtensions
    {
        public static async Task<T> AddIfNotExistsAsync<T>(this DbSet<T> dbSet, T entity, Expression<Func<T, bool>> predicate = null) where T : class, new()
        {
            bool exists;
            if (predicate != null)
                exists = await dbSet.AnyAsync(predicate);
            else
                exists = await dbSet.AnyAsync();

            if (!exists)
            {
                dbSet.Add(entity);
                return entity;
            }
            else
                return entity;

        }
    }

    public class ResourceFoundException : Exception
    {

    }
}
