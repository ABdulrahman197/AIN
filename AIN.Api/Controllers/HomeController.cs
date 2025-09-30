using Microsoft.AspNetCore.Mvc;

namespace AIN.Api.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}


