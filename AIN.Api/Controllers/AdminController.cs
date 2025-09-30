using AIN.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIN.Api.Controllers
{
    [Authorize(Policy = "RequireAdmin")]
	public class AdminController : Controller
	{
		private readonly IReportService _reportService;
		private readonly IAdminService _adminService;

		public AdminController(IReportService reportService, IAdminService adminService)
		{
			_reportService = reportService;
			_adminService = adminService;
		}

		// GET: /Admin
		public async Task<IActionResult> Index(
			int page = 1,
			int pageSize = 20,
			string? status = null,
			string? category = null,
			string? priority = null,
			string? search = null,
			CancellationToken ct = default)
		{
			var reports = await _reportService.ListAllAsync(page, pageSize, ct);
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;
			ViewBag.Status = status;
			ViewBag.Category = category;
			ViewBag.Priority = priority;
			ViewBag.Search = search;
			ViewBag.Total = reports.Count();
			return View(reports);
		}

		// GET: /Admin/Details/{id}
		public async Task<IActionResult> Details(Guid id, CancellationToken ct)
		{
			var report = await _reportService.GetWithInteractionsAsync(id, null, ct);
			if (report == null) return NotFound();
			return View(report);
		}

		// GET: /Admin/Users
        public async Task<IActionResult> Users(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default)
		{
            // Fetch all, then filter + paginate in-memory for simplicity
            var all = await _adminService.GetAllUsersAsync(0, int.MaxValue, ct);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                all = all.Where(u =>
                    (!string.IsNullOrEmpty(u.DisplayName) && u.DisplayName.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(u.Email) && u.Email.Contains(s, StringComparison.OrdinalIgnoreCase))
                );
            }

            var total = all.Count();
            var skip = Math.Max(0, (page - 1) * pageSize);
            var pageItems = all.Skip(skip).Take(pageSize).ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.Search = search;
            return View(pageItems);
		}

		// GET: /Admin/User/{id}
		public async Task<IActionResult> User(Guid id, CancellationToken ct = default)
		{
			var user = await _adminService.GetUserByIdAsync(id, ct);
			if (user == null) return NotFound();
			return View(user);
		}

		// POST: /Admin/User/{id}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateUser(Guid id, AIN.Application.Dtos.UserUpdateRequest request, CancellationToken ct = default)
		{
			var ok = await _adminService.UpdateUserAsync(id, request, ct);
			if (!ok) return NotFound();
			return RedirectToAction(nameof(User), new { id });
		}

		// POST: /Admin/User/Delete/{id}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct = default)
		{
			var ok = await _adminService.DeleteUserAsync(id, ct);
			if (!ok) return NotFound();
			return RedirectToAction(nameof(Users));
		}
	}
}


