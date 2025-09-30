using AIN.Application.Dtos;
using AIN.Application.Interfaces.IRepos;
using AIN.Application.Interfaces.IServices;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using static AIN.Core.Enums.enums;
using AIN.Application.Helpers;

namespace AIN.Api.Controllers.Api
{
	[ApiController]
	[Route("api")]
	public class ReportsController : ControllerBase
	{
		private readonly IReportService _reports;
		private readonly ILikeService _likes;
		private readonly ICommentService _comments;
		private readonly IReportRepo _reportRepo;
		
		private readonly IConfiguration _config;

		public ReportsController(IReportService reports, ILikeService likes, ICommentService comments, IReportRepo reportRepo, IConfiguration config)
		{
			_reports = reports;
			_likes = likes;
			_comments = comments;
			_reportRepo = reportRepo;
			
			_config = config;
		}

		[HttpPost("reports")]
		public async Task<IActionResult> CreateReport([FromBody] ReportCreateRequest req, CancellationToken ct)
		{
			var entity = await _reports.CreateAsync(req, ct);
			
			return Created($"/api/reports/{entity.Id}", new { entity.Id });
		}

		[HttpGet("reports/{id:guid}")]
		public async Task<IActionResult> GetReport(Guid id, CancellationToken ct)
		{
			

			var entity = await _reports.GetAsync(id, ct);
			if (entity == null) return NotFound();

			string ToUrl(string path) => $"/uploads/{System.IO.Path.GetFileName(path)}";
			var response = entity.ToResponse(ToUrl);

			
			return Ok(response);
		}

		[HttpGet("feed")]
		public async Task<IActionResult> GetFeed([FromQuery] int page, [FromQuery] int pageSize, CancellationToken ct)
		{
			page = page == 0 ? 1 : page;
			pageSize = pageSize == 0 ? 20 : Math.Min(pageSize, 100);
			
			var list = await _reports.GetPublicFeedAsync(page, pageSize, ct);
			string ToUrl(string path) => $"/uploads/{System.IO.Path.GetFileName(path)}";
			var response = list.Select(r => r.ToResponse(ToUrl));
			return Ok(response);
		}

		[HttpPatch("reports/{id:guid}/status")]
		public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] ReportStatus status, CancellationToken ct)
		{
			await _reports.UpdateStatusAsync(id, status, ct);
		
			return NoContent();
		}

		[HttpGet("reports/{id:guid}/interactions")]
		public async Task<IActionResult> GetInteractions(Guid id, CancellationToken ct)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			Guid? currentUserId = Guid.TryParse(userIdClaim, out var parsedId) ? parsedId : null;
			var report = await _reports.GetWithInteractionsAsync(id, currentUserId, ct);
			if (report == null) return NotFound();
			return Ok(report);
		}

		[Authorize]
		[HttpPut("reports/{id:guid}")]
		public async Task<IActionResult> Update(Guid id, [FromBody] ReportUpdateRequest request, CancellationToken ct)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();
			var report = await _reports.UpdateAsync(id, request, userId, ct);
			if (report == null) return NotFound();
		
			return Ok(new { message = "Report updated successfully" });
		}

		[Authorize]
		[HttpDelete("reports/{id:guid}")]
		public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();
			var success = await _reports.DeleteAsync(id, userId, ct);
			if (!success) return NotFound();
		
			return NoContent();
		}

		[Authorize]
		[HttpPost("reports/{id:guid}/like")]
		public async Task<IActionResult> ToggleLike(Guid id, CancellationToken ct)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();
			var likeRequest = new LikeRequest { ReportId = id };
			var isLiked = await _likes.ToggleLikeAsync(userId, likeRequest, ct);
		
			return Ok(new { isLiked, message = isLiked ? "Report liked" : "Report unliked" });
		}

		[Authorize]
		[HttpPost("reports/{id:guid}/comments")]
		public async Task<IActionResult> AddComment(Guid id, [FromBody] CommentRequest request, CancellationToken ct)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();
			request.ReportId = id;
			var comment = await _comments.AddCommentAsync(userId, request, ct);
			
			return Created($"/api/comments/{comment.Id}", comment);
		}

		[HttpGet("reports/{id:guid}/comments")]
		public async Task<IActionResult> GetComments(Guid id, [FromQuery] int page, [FromQuery] int pageSize, CancellationToken ct)
		{
			page = page == 0 ? 1 : page;
			pageSize = pageSize == 0 ? 20 : Math.Min(pageSize, 100);
			var skip = (page - 1) * pageSize;
			var comments = await _comments.GetCommentsAsync(id, skip, pageSize, ct);
			return Ok(comments);
		}

		[Authorize]
		[HttpPut("comments/{commentId:guid}")]
		public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] CommentUpdateRequest request, CancellationToken ct)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();
			var comment = await _comments.UpdateCommentAsync(userId, commentId, request, ct);
			if (comment == null) return NotFound();
			
			return Ok(comment);
		}

		[Authorize]
		[HttpDelete("comments/{commentId:guid}")]
		public async Task<IActionResult> DeleteComment(Guid commentId, CancellationToken ct)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();
			var success = await _comments.DeleteCommentAsync(userId, commentId, ct);
			if (!success) return NotFound();
		
			return NoContent();
		}




        [HttpGet("users/{id:guid}/reports")]
		public async Task<IActionResult> GetUserReports(Guid id, [FromQuery] int page, [FromQuery] int pageSize, CancellationToken ct)
		{
			page = page == 0 ? 1 : page;
			pageSize = pageSize == 0 ? 20 : Math.Min(pageSize, 100);
			var skip = (page - 1) * pageSize;
			var reports = await _reports.GetUserReportsAsync(id, skip, pageSize, ct);
			string ToUrl(string path) => $"/uploads/{System.IO.Path.GetFileName(path)}";
			var response = reports.Select(r => r.ToResponse(ToUrl));
			return Ok(response);
        }
    }
}


