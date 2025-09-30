using AIN.Application.Dtos;
using AIN.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIN.Api.Controllers.Api
{
	[ApiController]
	[Route("api/admin")]
	[Authorize(Policy = "RequireAdmin")] // adjust as needed
	public class AdminController : ControllerBase
	{
		private readonly IAdminService _admin;

		public AdminController(IAdminService admin)
		{
			_admin = admin;
		}

		[HttpGet("users")]
		public async Task<IActionResult> GetUsers([FromQuery] int page, [FromQuery] int pageSize, CancellationToken ct)
		{
			page = page == 0 ? 1 : page;
			pageSize = pageSize == 0 ? 20 : Math.Min(pageSize, 100);
			var skip = (page - 1) * pageSize;
			var users = await _admin.GetAllUsersAsync(skip, pageSize, ct);
			var totalCount = await _admin.GetTotalUsersCountAsync(ct);
			return Ok(new
			{
				users,
				totalCount,
				page,
				pageSize,
				totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
			});
		}

		[HttpGet("users/{userId:guid}")]
		public async Task<IActionResult> GetUser(Guid userId, CancellationToken ct)
		{
			var user = await _admin.GetUserByIdAsync(userId, ct);
			if (user == null) return NotFound();
			return Ok(user);
		}

		[HttpPut("users/{userId:guid}")]
		public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UserUpdateRequest request, CancellationToken ct)
		{
			var success = await _admin.UpdateUserAsync(userId, request, ct);
			if (!success) return NotFound();
			return Ok(new { message = "User updated successfully" });
		}

		[HttpDelete("users/{userId:guid}")]
		public async Task<IActionResult> DeleteUser(Guid userId, CancellationToken ct)
		{
			var success = await _admin.DeleteUserAsync(userId, ct);
			if (!success) return NotFound();
			return NoContent();
		}
	}
}


