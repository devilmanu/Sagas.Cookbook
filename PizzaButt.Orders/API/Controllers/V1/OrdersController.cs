using MassTransit;
using Microsoft.AspNetCore.Mvc;
using PizzaButt.Orders.Services.Oders;
using PizzaButt.Orders.Services.Oders.Dtos;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PizzaButt.Orders.API.Controllers.V1
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _orderService;

        public OrdersController(IOrdersService orderService)
        {
            _orderService = orderService;
        }

        //GET: api/<Orders>
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
        {
            var response = await _orderService.GetOrdersAsync(cancellationToken);
            return Ok(response);
        }

        // GET api/<Orders>/123123-...
        [HttpDelete]
        public async Task<IActionResult> Delete(CancellationToken cancellationToken = default)
        {
            await _orderService.DeleteAllAsync(cancellationToken);
            return NoContent();
        }
        

        // POST api/<OrdersController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OrderDtoRequest value, CancellationToken cancellationToken = default)
        {
            await _orderService.SubmmitOrderAsync(value, cancellationToken);
            return Accepted();
        }
    }
}
