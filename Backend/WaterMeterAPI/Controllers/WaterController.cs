using Microsoft.AspNetCore.Mvc;
using WaterMeterAPI.Data;
using WaterMeterAPI.Models;

namespace WaterMeterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WaterController : ControllerBase
    {
        private readonly FirebirdHelper _fbHelper;

        public WaterController(FirebirdHelper fbHelper)
        {
            _fbHelper = fbHelper;
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var readings = _fbHelper.GetAllWaterReadings();
            return Ok(readings);
        }

        [HttpPost("add")]
        public IActionResult Add([FromBody] WaterReading reading)
        {
            _fbHelper.AddWaterReading(reading);
            return Ok();
        }
    }
}