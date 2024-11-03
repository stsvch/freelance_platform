using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class ResponseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
