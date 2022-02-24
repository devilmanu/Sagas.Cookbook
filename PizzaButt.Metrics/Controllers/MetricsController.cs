using Microsoft.AspNetCore.Mvc;
using PizzaButt.Mdetrics.Controllers;

namespace PizzaButt.Metrics.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MetricsController : ControllerBase
    {

        private readonly ILogger<MetricsController> _logger;

        public MetricsController(ILogger<MetricsController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public ActionResult<MetricsDto> Post([FromBody] MetricsDto mertricsDto)
        {
            return Created("",mertricsDto);
        }



    }
}