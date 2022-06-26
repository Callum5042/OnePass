using Microsoft.AspNetCore.Mvc;
using OnePass.Services;

namespace OnePass.Web.API
{
    [ApiController]
    [Route("api/v1/password/generate")]
    public class PasswordGenerateController : ControllerBase
    {
        // https://localhost:7104/api/v1/password/generate?minLength=10&maxLength=20&uppercase=true&lowercase=true&numbers=true&symbols=false
        [HttpGet]
        public IActionResult Get(int minLength, int maxLength, bool uppercase, bool lowercase, bool numbers, bool symbols)
        {
            var generator = new PasswordGenerator()
            {
                MinLength = minLength,
                MaxLength = maxLength,
                HasUppercase = uppercase,
                HasLowercase = lowercase,
                HasNumbers = numbers,
                HasSymbols = symbols
            };

            var password = generator.Generate();
            return Ok(new PasswordResult(password));
        }

        private record PasswordResult(string Password);
    }
}
