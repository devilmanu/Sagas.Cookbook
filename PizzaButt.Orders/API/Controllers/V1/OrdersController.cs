using MassTransit;
using Microsoft.AspNetCore.Mvc;
using PizzaButt.Orders.Infrastructure.Sagas;
using PizzaButt.Orders.Services.Oders;
using PizzaButt.Orders.Services.Oders.Dtos;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

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


        //GET: api/<Orders>/id
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var response = await _orderService.GetOrderByIdAsync(id, cancellationToken);
            Activity.Current?.AddTag("Orders.Get.id", id.ToString());
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
            Activity.Current?.AddTag("Orders.Post.OrderDtoRequest", JsonSerializer.Serialize(value, new JsonSerializerOptions
            {
                WriteIndented = true,
            }));
            return Accepted();
        }
    }
}
