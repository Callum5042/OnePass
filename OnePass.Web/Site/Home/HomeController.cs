using Microsoft.AspNetCore.Mvc;

namespace OnePass.Web.Site.Home
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
