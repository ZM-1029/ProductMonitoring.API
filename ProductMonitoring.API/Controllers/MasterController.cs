using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ProductMonitoring.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        public MasterController()
        {
            
        }
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Master Controller is working");
        }
    }
}
