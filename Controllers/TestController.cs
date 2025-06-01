using Microsoft.AspNetCore.Mvc;

namespace EmergencyComm.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> Get()
        {
            return Ok(new
            {
                Message = "Emergency Comm API is working!",
                Timestamp = DateTime.UtcNow,
                Status = "Success"
            });
        }

        [HttpGet("health")]
        public ActionResult<string> Health()
        {
            return Ok(new
            {
                Status = "Healthy",
                Service = "Emergency Communication API",
                Version = "1.0.0"
            });
        }
    }
}