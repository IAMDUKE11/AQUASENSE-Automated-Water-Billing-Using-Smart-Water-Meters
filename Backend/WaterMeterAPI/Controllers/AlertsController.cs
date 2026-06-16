using Microsoft.AspNetCore.Mvc;
using WaterMeterAPI.Data;
using WaterMeterAPI.Models;

namespace WaterMeterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly FirebirdHelper _fbHelper;

        public AlertsController(FirebirdHelper fbHelper)
        {
            _fbHelper = fbHelper;
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var alerts = _fbHelper.GetAllAlerts();
            return Ok(alerts);
        }

        [HttpPost("add")]
        public IActionResult Add([FromBody] Alert alert)
        {
            _fbHelper.AddAlert(alert);
            return Ok();
        }
    }
}