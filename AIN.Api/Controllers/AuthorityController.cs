using AIN.Application.Interfaces.IRepos;
using AIN.Application.Interfaces.IServices;
using AIN.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIN.Api.Controllers
{
    [Authorize(Policy = "RequireAuthority")]
    public class AuthorityController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IAuthorityRepo _authorityRepo;
        private readonly ILikeService _likeService;
        private readonly ICommentService _commentService;

        public AuthorityController(IReportService reportService, IAuthorityRepo authorityRepo, ILikeService likeService, ICommentService commentService)
        {
            _reportService = reportService;
            _authorityRepo = authorityRepo;
            _likeService = likeService;
            _commentService = commentService;
        }

        // GET: /Authority
        public async Task<IActionResult> Index(string? searchTerm = null, AIN.Core.Enums.enums.ReportStatus? statusFilter = null, 
            AIN.Core.Enums.enums.ReportCategory? categoryFilter = null, DateTime? startDate = null, DateTime? endDate = null, 
            int page = 1, int pageSize = 50, CancellationToken ct = default)
        {
            var allAuthorities = await _authorityRepo.ListAsync(ct);

            Guid? authorityId = null;

            // 1) authorityId claim
            var authorityIdClaim = User.FindFirst("authorityId")?.Value;
            if (Guid.TryParse(authorityIdClaim, out var parsedAuthorityId))
            {
                authorityId = parsedAuthorityId;
            }

            // 2) match by email
            if (authorityId == null)
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var byEmail = string.IsNullOrWhiteSpace(email) ? null : allAuthorities.FirstOrDefault(a => a.ContactEmail == email);
                if (byEmail != null) authorityId = byEmail.Id;
            }

            // 3) match by display name
            if (authorityId == null)
            {
                var displayName = User.FindFirst("displayName")?.Value;
                var byName = string.IsNullOrWhiteSpace(displayName) ? null : allAuthorities.FirstOrDefault(a => a.Name == displayName);
                if (byName != null) authorityId = byName.Id;
            }

            IEnumerable<AIN.Core.Entites.Report> reports;

            if (authorityId.HasValue)
            {
                reports = await _reportService.ListByAuthorityAsync(authorityId.Value, page, pageSize, ct);
            }
            else
            {
                ViewBag.Warning = "لم يتم ربط المستخدم بجهة محددة، تم عرض جميع البلاغات للفحص.";
                reports = await _reportService.ListAllAsync(page, pageSize, ct);
            }

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                reports = reports.Where(r => 
                    r.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (r.Reporter?.DisplayName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (statusFilter.HasValue)
            {
                reports = reports.Where(r => r.Status == statusFilter.Value);
            }

            if (categoryFilter.HasValue)
            {
                reports = reports.Where(r => r.Category == categoryFilter.Value);
            }

            if (startDate.HasValue)
            {
                reports = reports.Where(r => r.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                reports = reports.Where(r => r.CreatedAt <= endDate.Value);
            }

            // Create summary
            var summary = new ReportStatusSummary
            {
                TotalReports = reports.Count(),
                PendingCount = reports.Count(r => r.Status == AIN.Core.Enums.enums.ReportStatus.Pending),
                InReviewCount = reports.Count(r => r.Status == AIN.Core.Enums.enums.ReportStatus.InReview),
                DispatchedCount = reports.Count(r => r.Status == AIN.Core.Enums.enums.ReportStatus.Dispatched),
                ResolvedCount = reports.Count(r => r.Status == AIN.Core.Enums.enums.ReportStatus.Resolved),
                RejectedCount = reports.Count(r => r.Status == AIN.Core.Enums.enums.ReportStatus.Rejected)
            };

            var filter = new ReportFilter
            {
                SearchTerm = searchTerm,
                StatusFilter = statusFilter,
                CategoryFilter = categoryFilter,
                StartDate = startDate,
                EndDate = endDate
            };

            var viewModel = new AuthorityDashboardViewModel
            {
                Reports = reports,
                Summary = summary,
                Filter = filter
            };

            return View(viewModel);
        }

        // GET: /Authority/Details/{id}
        public async Task<IActionResult> Details(Guid id, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? currentUserId = Guid.TryParse(userIdClaim, out var parsedId) ? parsedId : null;
            var report = await _reportService.GetWithInteractionsAsync(id, currentUserId, ct);
            if (report == null) return NotFound();
            return View(report);
        }

        // POST: /Authority/Status/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Status(Guid id, AIN.Core.Enums.enums.ReportStatus status, CancellationToken ct)
        {
            await _reportService.UpdateStatusAsync(id, status, ct);
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Authority/UpdateStatus/{id} - AJAX endpoint
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(Guid id, AIN.Core.Enums.enums.ReportStatus status, CancellationToken ct)
        {
            try
            {
                await _reportService.UpdateStatusAsync(id, status, ct);
                return Json(new { success = true, message = "تم تحديث حالة البلاغ بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ في تحديث حالة البلاغ: " + ex.Message });
            }
        }

        // POST: /Authority/Comment/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comment(Guid id, string content, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();
            var req = new AIN.Application.Dtos.CommentRequest { ReportId = id, Content = content };
            await _commentService.AddCommentAsync(userId, req, ct);
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Authority/Like/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(Guid id, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();
            var likeReq = new AIN.Application.Dtos.LikeRequest { ReportId = id };
            await _likeService.ToggleLikeAsync(userId, likeReq, ct);
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}


