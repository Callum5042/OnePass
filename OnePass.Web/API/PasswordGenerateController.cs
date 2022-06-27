using Microsoft.AspNetCore.Mvc;
using OnePass.Services;

namespace OnePass.Web.API
{
    [ApiController]
    [Route("api/v1/password/generate")]
    public class PasswordGenerateController : ControllerBase
    {
        // https://localhost:7104/api/v1/password/generate?amount=5&minLength=10&maxLength=20&uppercase=true&lowercase=true&numbers=true&symbols=false
        [HttpGet]
        public IActionResult Get(int amount, int minLength, int maxLength, bool uppercase, bool lowercase, bool numbers, bool symbols)
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

            var passwords = new List<PasswordResult>();
            for (int i = 0; i < amount; i++)
            {
                var password = generator.Generate();
                passwords.Add(new PasswordResult(password));
            }

            return Ok(passwords);
        }

        private record PasswordResult(string Password);
    }
}
