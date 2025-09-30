using Microsoft.AspNetCore.Mvc;

namespace dashboard.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
