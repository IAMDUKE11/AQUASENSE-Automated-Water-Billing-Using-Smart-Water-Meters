using Microsoft.AspNetCore.Mvc;
using WaterMeterAPI.Data;
using WaterMeterAPI.Models;

namespace WaterMeterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly FirebirdHelper _fbHelper;

        public UsersController(FirebirdHelper fbHelper)
        {
            _fbHelper = fbHelper;
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var users = _fbHelper.GetAllUsers();
            return Ok(users);
        }

        [HttpPost("add")]
        public IActionResult Add([FromBody] User user)
        {
            _fbHelper.AddUser(user);
            return Ok();
        }
    }
}