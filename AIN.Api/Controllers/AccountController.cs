using AIN.Application.Dtos;
using AIN.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using static AIN.Core.Enums.enums;


namespace AIN.Api.Controllers
{
	public class AccountController : Controller
	{
	private readonly IAuthService _auth;
	private readonly IConfiguration _config;

	public AccountController(IAuthService auth, IConfiguration config)
	{
		_auth = auth;
		_config = config;
	}

		[HttpGet]
		public IActionResult Login(string? returnUrl = null)
		{
			ViewBag.ReturnUrl = returnUrl;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
	public async Task<IActionResult> Login(LoginRequest req, CancellationToken ct)
		{
		var jwtSection = _config.GetSection("Jwt");
			var key = jwtSection.GetValue<string>("Key") ?? string.Empty;
			var issuer = jwtSection.GetValue<string>("Issuer") ?? string.Empty;
			var audience = jwtSection.GetValue<string>("Audience") ?? string.Empty;
			var expiry = jwtSection.GetValue<int>("ExpiryInHours");

		var result = await _auth.LoginAsync(req, key, issuer, audience, expiry, ct);
			if (result == null)
			{
				ModelState.AddModelError(string.Empty, "بيانات الدخول غير صحيحة");
				return View();
			}

			var user = result.Value.user;
			if ( ! (user.Role == UserRole.Admin || user.Role == UserRole.Authority) )
			{
				ModelState.AddModelError(string.Empty, "غير مسموح بالدخول إلا للإداري والجهة فقط");
				return View();
			}

			Response.Cookies.Append("token", result.Value.token, new CookieOptions
			{
				HttpOnly = true,
				Secure = Request.IsHttps,
				SameSite = SameSiteMode.Lax,
				Expires = DateTimeOffset.UtcNow.AddHours(expiry)
			});

			var returnUrl = Request.Query["returnUrl"].ToString();
			if (!string.IsNullOrEmpty(returnUrl))
				return Redirect(returnUrl);

            return Redirect(user.Role == UserRole.Admin ? "/Admin" : "/Authority");

        }

        [HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Logout()
		{
			Response.Cookies.Delete("token");
			return RedirectToAction("Login");
		}
	}
}


