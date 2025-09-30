using AIN.Application.Dtos;
using AIN.Application.Interfaces.IRepos;
using AIN.Application.Interfaces.IServices;

using AIN.Infrastructrue.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;



namespace AIN.Api.Controllers.Api
{
	[ApiController]
	[Route("api")]
	public class UsersController : ControllerBase
	{
		private readonly IAuthorityRepo _authorityRepo;
		private readonly ITrustPointsService _trustPoints;
		private readonly IReportRepo _reportRepo;
		private readonly AinDbContext _db;
		private readonly IConfiguration _config;
		

		public UsersController(IAuthorityRepo authorityRepo, ITrustPointsService trustPoints, IReportRepo reportRepo, AinDbContext db, IConfiguration config)
		{
			_authorityRepo = authorityRepo;
			_trustPoints = trustPoints;
			_reportRepo = reportRepo;
			_db = db;
			_config = config;
			
		}

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] ProfileUpdateRequest request, [FromServices] IUserRepo users, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var user = await users.GetByIdAsync(userId, ct);
            if (user == null) return NotFound();

            user.DisplayName = request.DisplayName;
            await users.UpdateAsync(user, ct);
            await users.SaveChangesAsync(ct);

            return Ok(new { message = "Profile updated", user = new { user.Id, user.Email, user.DisplayName, user.TrustPoints, user.Badge } });
        }

        [HttpGet("authorities")]
		public async Task<IActionResult> GetAuthorities(CancellationToken ct)
		{
			
			var list = await _authorityRepo.ListAsync(ct);
			var response = list.Select(a => new { a.Id, a.Name, a.Department });
			
			return Ok(response);
		}



		[HttpPost("users/{id:guid}/trustpoints")]
		public async Task<IActionResult> AddTrustPoints(Guid id, [FromQuery] int delta, CancellationToken ct)
		{
			var total = await _trustPoints.AddPointsAsync(id, delta, ct);
			
			return Ok(new { userId = id, total });
		}

		[HttpPost("reports/{id:guid}/attachments")]
		[DisableRequestSizeLimit]
		public async Task<IActionResult> UploadAttachment(Guid id, IFormFile file, CancellationToken ct)
		{
			if (file == null || file.Length == 0) return BadRequest("Empty file");
			var report = await _reportRepo.GetByIdAsync(id, includeAttachments: false, ct);
			if (report == null) return NotFound();
			var uploads = _config.GetValue<string>("Storage:UploadsRoot") ?? "wwwroot/uploads";
			Directory.CreateDirectory(uploads);
			var safeName = Path.GetFileName(file.FileName);
			var storedName = $"{Guid.NewGuid()}_{safeName}";
			var fullPath = Path.Combine(uploads, storedName);
			await using (var stream = System.IO.File.Create(fullPath))
				await file.CopyToAsync(stream, ct);
			var attachment = new AIN.Core.Entites.Attachment
			{
				Id = Guid.NewGuid(),
				ReportId = id,
				FileName = safeName,
				ContentType = file.ContentType,
				SizeBytes = file.Length,
				StoragePath = fullPath
			};
			_db.Attachments.Add(attachment);
			await _db.SaveChangesAsync(ct);
			var url = $"/uploads/{storedName}";
			
			return Created(url, new { attachment.Id, url });
		}
	}
}


