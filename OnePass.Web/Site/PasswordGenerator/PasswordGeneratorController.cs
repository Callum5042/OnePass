using Microsoft.AspNetCore.Mvc;

namespace OnePass.Web.Site.PasswordGenerator
{
    public class PasswordGeneratorController : Controller
    {
        [Route("generate-password")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
