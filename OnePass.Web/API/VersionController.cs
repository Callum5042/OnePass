using Microsoft.AspNetCore.Mvc;

namespace OnePass.Web.API
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var version = "1.0.4455";
            return Ok(new VersionResult(version));
        }

        private record VersionResult(string Version);
    }
}
