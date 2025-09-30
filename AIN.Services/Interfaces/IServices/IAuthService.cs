using AIN.Application.Dtos;
using AIN.Core.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIN.Application.Interfaces.IServices
{
    public interface IAuthService
    {
        Task<UserAccount> RegisterAsync(UserCreateRequest request, CancellationToken ct = default);
        Task<(string token, UserAccount user)?> LoginAsync(LoginRequest request, string jwtKey, string issuer, string audience, int expiryHours, CancellationToken ct = default);
        Task<UserAccount?> GetCurrentUserAsync(string userId, CancellationToken ct = default);
        Task<bool> ForgetPasswordAsync(string email, CancellationToken ct = default);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
        Task<(string token, string refreshToken)?> RefreshTokenAsync(string refreshToken, string jwtKey, string issuer, string audience, int expiryHours, CancellationToken ct = default);
    }
}
