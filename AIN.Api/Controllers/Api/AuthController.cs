using AIN.Application.Dtos;
using AIN.Application.Interfaces.IRepos;
using AIN.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIN.Api.Controllers.Api
{
	[ApiController]
	[Route("api/auth")]
	public class AuthController : ControllerBase
	{
		[HttpPost("register")]
		public async Task<IActionResult> Register(UserCreateRequest req, IAuthService auth, CancellationToken ct)
		{
			try
			{
				var user = await auth.RegisterAsync(req, ct);
				return Created($"/api/users/{user.Id}", new { user.Id, user.Email, user.DisplayName });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpPost("verify-otp")]
		public async Task<IActionResult> VerifyOtp(OtpVerificationDto dto, IUserRepo repo, CancellationToken ct)
		{
			var user = await repo.GetByEmailAsync(dto.Email, ct);
			if (user == null) return NotFound("User not found");
			if (user.OtpCode != dto.Otp || user.OtpExpiryTime < DateTime.UtcNow)
				return BadRequest("Invalid or expired OTP");
			user.IsEmailConfirmed = true;
			user.OtpCode = null;
			user.OtpExpiryTime = null;
			await repo.UpdateAsync(user, ct);
			return Ok("OTP verified!");
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login(LoginRequest req, IAuthService auth, IConfiguration config, CancellationToken ct)
		{
			var jwtSection = config.GetSection("Jwt");
			var key = jwtSection.GetValue<string>("Key") ?? string.Empty;
			var issuer = jwtSection.GetValue<string>("Issuer") ?? string.Empty;
			var audience = jwtSection.GetValue<string>("Audience") ?? string.Empty;
			var expiry = jwtSection.GetValue<int>("ExpiryInHours");
			var result = await auth.LoginAsync(req, key, issuer, audience, expiry, ct);
			if (result == null) return Unauthorized();
			return Ok(new { token = result.Value.token, userId = result.Value.user.Id, refreshToken = result.Value.user.RefreshToken, expiry = DateTime.UtcNow.AddHours(expiry) });
		}

		[Authorize]
		[HttpGet("me")]
		public async Task<IActionResult> Me(IAuthService auth, CancellationToken ct)
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId)) return Unauthorized();
			var user = await auth.GetCurrentUserAsync(userId, ct);
			if (user == null) return NotFound("User not found");
			return Ok(new UserResponse(user.Id, user.Email, user.DisplayName, user.TrustPoints, user.Badge));
		}

		[HttpPost("forget-password")]
		public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest req, IAuthService auth, CancellationToken ct)
		{
			var result = await auth.ForgetPasswordAsync(req.Email, ct);
			if (!result) return BadRequest("Email not found or invalid");
			return Ok(new { message = "Password reset code sent to your email" });
		}

		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword(ResetPasswordRequest req, IAuthService auth, CancellationToken ct)
		{
			var result = await auth.ResetPasswordAsync(req, ct);
			if (!result) return BadRequest("Invalid OTP or expired");
			return Ok(new { message = "Password reset successfully" });
		}

		[HttpPost("refresh-token")]
		public async Task<IActionResult> Refresh(RefreshTokenRequest req, IAuthService auth, IConfiguration config, CancellationToken ct)
		{
			var jwtSection = config.GetSection("Jwt");
			var key = jwtSection.GetValue<string>("Key") ?? string.Empty;
			var issuer = jwtSection.GetValue<string>("Issuer") ?? string.Empty;
			var audience = jwtSection.GetValue<string>("Audience") ?? string.Empty;
			var expiry = jwtSection.GetValue<int>("ExpiryInHours");
			var result = await auth.RefreshTokenAsync(req.RefreshToken, key, issuer, audience, expiry, ct);
			if (result == null) return Unauthorized();
			return Ok(new { token = result.Value.token, refreshToken = result.Value.refreshToken, expiry = DateTime.UtcNow.AddHours(expiry) });
		}
	}
}


