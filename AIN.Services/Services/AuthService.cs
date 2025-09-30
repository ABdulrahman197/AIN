using AIN.Application.Dtos;
using AIN.Application.Interfaces.IRepos;
using AIN.Application.Interfaces.IServices;
using AIN.Core.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static AIN.Core.Enums.enums;
using System.Security.Cryptography;

namespace AIN.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepo _users;
        private readonly IEmailService _email;

        public AuthService(IUserRepo users , IEmailService emailService)
        {
            _users = users;
            _email = emailService;
        }
        public async Task<(string token, UserAccount user)?> LoginAsync(LoginRequest request, string jwtKey, string issuer, string audience, int expiryHours, CancellationToken ct = default)
        {
            var user = await _users.GetByEmailAsync(request.Email, ct);
            if (user == null) return null;
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return null;

            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey));
            var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);
            #region Claims 
            var claims = new System.Security.Claims.Claim[]
               {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
            new System.Security.Claims.Claim("displayName", user.DisplayName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
               };
            #endregion

            #region Token
            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                    issuer,
                    audience,
                    claims,
                    expires: DateTime.UtcNow.AddHours(expiryHours),
                    signingCredentials: creds
                ); 
            #endregion
            var tokenString = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);

            // Generate refresh token
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _users.UpdateAsync(user, ct);
            await _users.SaveChangesAsync(ct);

            return (tokenString, user);
        }

        public async Task<UserAccount> RegisterAsync(UserCreateRequest request, CancellationToken ct = default)
        {
            var existing = await _users.GetByEmailAsync(request.Email, ct);
            if (existing != null)
                throw new InvalidOperationException("Email already registered");

            var user = new UserAccount
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                DisplayName = request.DisplayName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                TrustPoints = 0,
                Badge = TrustBadge.Newcomer,

                // 🔹 New fields
                IsEmailConfirmed = false,
                OtpCode = GenerateOtp(),
                OtpExpiryTime = DateTime.UtcNow.AddMinutes(15)
            };

            await _users.AddAsync(user, ct);
            await _users.SaveChangesAsync(ct);

            // 🔹 Send OTP via email service
            await _email.SendAsync(
                user.Email,
                "Your OTP Code",
                $"Welcome {user.DisplayName},\n\nYour OTP is: {user.OtpCode}\n\nThis code expires in 5 minutes."
            );

            return user;
        }

        public async Task<UserAccount?> GetCurrentUserAsync(string userId, CancellationToken ct = default)
        {
            if (!Guid.TryParse(userId, out var userGuid))
                return null;

            return await _users.GetByIdAsync(userGuid, ct);
        }

        public async Task<bool> ForgetPasswordAsync(string email, CancellationToken ct = default)
        {
            var user = await _users.GetByEmailAsync(email, ct);
            if (user == null) return false;

            // Generate OTP for password reset
            user.OtpCode = GenerateOtp();
            user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(15);

            await _users.UpdateAsync(user, ct);
            await _users.SaveChangesAsync(ct);

            // Send OTP via email
            await _email.SendAsync(
                user.Email,
                "Password Reset Code",
                $"Hello {user.DisplayName},\n\nYour password reset code is: {user.OtpCode}\n\nThis code expires in 15 minutes.\n\nIf you didn't request this, please ignore this email."
            );

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
        {
            var user = await _users.GetByEmailAsync(request.Email, ct);
            if (user == null) return false;

            if ( user.OtpExpiryTime < DateTime.UtcNow)
                return false;

            // Reset password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
           
            user.OtpExpiryTime = null;

            await _users.UpdateAsync(user, ct);
            await _users.SaveChangesAsync(ct);

            return true;
        }

        public async Task<(string token, string refreshToken)?> RefreshTokenAsync(string refreshToken, string jwtKey, string issuer, string audience, int expiryHours, CancellationToken ct = default)
        {
            // Find user by refresh token - this requires a method to search by refresh token
            // For now, we'll implement a simple approach by checking all users (not ideal for production)
            var allUsers = await _users.GetAllAsync(ct);
            var user = allUsers.FirstOrDefault(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiryTime > DateTime.UtcNow);
            
            if (user == null)
                return null;

            // Generate new access token
            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey));
            var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var claims = new System.Security.Claims.Claim[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
                new System.Security.Claims.Claim("displayName", user.DisplayName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddHours(expiryHours),
                signingCredentials: creds
            );

            var tokenString = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);

            // Generate new refresh token
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _users.UpdateAsync(user, ct);
            await _users.SaveChangesAsync(ct);

            return (tokenString, newRefreshToken);
        }

        // Helper methods
        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); 
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }


    }
}
