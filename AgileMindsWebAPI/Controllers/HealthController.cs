using Microsoft.AspNetCore.Mvc;

namespace AgileMindsWebAPI.Controllers
{
    public class HealthController : Controller
    {
        [Route("api/[controller]")]
        [HttpGet]
        public IActionResult GetHealth()
        {
            return Ok(new { Status = "Healthy" });
        }
    }
}
