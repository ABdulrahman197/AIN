using AIN.Application.Dtos;
using AIN.Application.Interfaces.IRepos;
using AIN.Application.Interfaces.IServices;
using System.Security.Claims;

namespace AIN.Api.Endpoints
{
    public static class AuthEndpoints
    {
        public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder app)
        {
            // Auth: Register
            app.MapPost("/register", async (UserCreateRequest req, IAuthService auth, CancellationToken ct) =>
            {
                try
                {
                    var user = await auth.RegisterAsync(req, ct);
                    return Results.Created($"/api/users/{user.Id}", new { user.Id, user.Email, user.DisplayName });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

            app.MapPost("/verify-otp", async (OtpVerificationDto dto, IUserRepo repo, CancellationToken ct) =>
            {
                var user = await repo.GetByEmailAsync(dto.Email, ct);
                if (user == null) return Results.NotFound("User not found");

                if (user.OtpCode != dto.Otp || user.OtpExpiryTime < DateTime.UtcNow)
                    return Results.BadRequest("Invalid or expired OTP");

                user.IsEmailConfirmed = true;
                user.OtpCode = null;
                user.OtpExpiryTime = null;

                await repo.UpdateAsync(user, ct);

                return Results.Ok("OTP verified!");
            });



            // Auth: Login
            app.MapPost("/login", async (LoginRequest req, IAuthService auth, IConfiguration config, CancellationToken ct) =>
            {
                var jwtSection = config.GetSection("Jwt");
                var key = jwtSection.GetValue<string>("Key") ?? string.Empty;
                var issuer = jwtSection.GetValue<string>("Issuer") ?? string.Empty;
                var audience = jwtSection.GetValue<string>("Audience") ?? string.Empty;
                var expiry = jwtSection.GetValue<int>("ExpiryInHours");

                var result = await auth.LoginAsync(req, key, issuer, audience, expiry, ct);
                if (result == null) return Results.Unauthorized();

                return Results.Ok(new { 
                    token = result.Value.token, 
                    userId = result.Value.user.Id,
                    refreshToken = result.Value.user.RefreshToken,
                    expiry = DateTime.UtcNow.AddHours(expiry)
                });
            });

            // Auth: Get Current User
            app.MapGet("/me", async (ClaimsPrincipal user, IAuthService auth, CancellationToken ct) =>
            {
                var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();

                var u = await auth.GetCurrentUserAsync(userId, ct);
                if (u == null)
                    return Results.NotFound("User not found");

                return Results.Ok(new UserResponse(
                    u.Id,
                    u.Email,
                    u.DisplayName,
                    u.TrustPoints,
                    u.Badge
                ));
            }).RequireAuthorization()
              .WithName("GetCurrentUser")
              .Produces<UserResponse>(StatusCodes.Status200OK)
              .Produces(StatusCodes.Status401Unauthorized)
              .Produces(StatusCodes.Status404NotFound);


            // Auth: Forget Password
            app.MapPost("/forget-password", async (ForgetPasswordRequest req, IAuthService auth, CancellationToken ct) =>
            {
                var result = await auth.ForgetPasswordAsync(req.Email, ct);
                if (!result)
                    return Results.BadRequest("Email not found or invalid");

                return Results.Ok(new { message = "Password reset code sent to your email" });
            });

            // Auth: Reset Password
            app.MapPost("/reset-password", async (ResetPasswordRequest req, IAuthService auth, CancellationToken ct) =>
            {
                var result = await auth.ResetPasswordAsync(req, ct);
                if (!result)
                    return Results.BadRequest("Invalid OTP or expired");

                return Results.Ok(new { message = "Password reset successfully" });
            });

            // Auth: Refresh Token
            app.MapPost("/refresh-token", async (RefreshTokenRequest req, IAuthService auth, IConfiguration config, CancellationToken ct) =>
            {
                var jwtSection = config.GetSection("Jwt");
                var key = jwtSection.GetValue<string>("Key") ?? string.Empty;
                var issuer = jwtSection.GetValue<string>("Issuer") ?? string.Empty;
                var audience = jwtSection.GetValue<string>("Audience") ?? string.Empty;
                var expiry = jwtSection.GetValue<int>("ExpiryInHours");

                var result = await auth.RefreshTokenAsync(req.RefreshToken, key, issuer, audience, expiry, ct);
                if (result == null)
                    return Results.Unauthorized();

                return Results.Ok(new {
                    token = result.Value.token,
                    refreshToken = result.Value.refreshToken,
                    expiry = DateTime.UtcNow.AddHours(expiry)
                });
            });

            return app; 
        }
    }
}
