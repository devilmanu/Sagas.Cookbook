using PizzaButt.Orders.Services.Oders.Dtos;

namespace PizzaButt.Orders.Services.Oders
{
    public interface IOrdersService
    {
        Task SubmmitOrderAsync(OrderDtoRequest request, CancellationToken cancellationToken);
        Task AcceptOrderAsync(OrderDtoRequest request, CancellationToken cancellationToken);
        Task ShipOrderAsync(OrderDtoRequest request, CancellationToken cancellationToken);
        Task FinishOrderAsync(OrderDtoRequest request, CancellationToken cancellationToken);
        Task RetriedOrderAsync(OrderDtoRequest request, CancellationToken cancellationToken);
        Task DeleteAllAsync(CancellationToken cancellationToken);
        Task<OrderDtoResponse> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<OrderDtoResponse>> GetOrdersAsync(CancellationToken cancellationToken);
        Task FailedOrderAsync(OrderDtoRequest request, CancellationToken cancellationToken);
    }
}