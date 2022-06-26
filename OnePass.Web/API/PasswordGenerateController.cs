using Microsoft.AspNetCore.Mvc;

namespace OnePass.Web.API
{
    [ApiController]
    [Route("api/v1/password/generate")]
    public class PasswordGenerateController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(bool complex)
        {
            var password = "Password123";
            return Ok(password);
        }
    }
}
