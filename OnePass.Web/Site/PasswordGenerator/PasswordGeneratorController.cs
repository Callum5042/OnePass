using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace OnePass.Web.Site.PasswordGenerator
{
    public class PasswordGeneratorController : Controller
    {
        [Route("generate-password")]
        public IActionResult Index(IndexQuery query)
        {
            var generator = new Services.PasswordGenerator()
            {
                MinLength = query.MinLength,
                MaxLength = query.MaxLength,
                HasUppercase = query.Uppercase,
                HasLowercase = query.Lowercase,
                HasNumbers = query.Numbers,
                HasSymbols = query.Symbols
            };

            var passwords = new List<string>();
            for (int i = 0; i < query.Amount; i++)
            {
                passwords.Add(generator.Generate());
            }

            var model = new IndexModel()
            {
                MinLength = query.MinLength,
                MaxLength = query.MaxLength,
                Uppercase = query.Uppercase,
                Lowercase = query.Lowercase,
                Numbers = query.Numbers,
                Symbols = query.Symbols,
                Passwords = string.Join(Environment.NewLine, passwords)
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Index(IndexModel model)
        {
            var query = new IndexQuery()
            {
                Amount = model.Amount
            };

            return RedirectToAction(nameof(Index), query);
        }
    }
}
