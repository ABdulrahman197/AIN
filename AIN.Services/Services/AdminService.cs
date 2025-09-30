using AIN.Application.Dtos;
using AIN.Application.Interfaces.IRepos;
using AIN.Application.Interfaces.IServices;
using AIN.Core.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIN.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepo _userRepo;
        private readonly IReportRepo _reportRepo;

        public AdminService(IUserRepo userRepo, IReportRepo reportRepo)
        {
            _userRepo = userRepo;
            _reportRepo = reportRepo;
        }

        public async Task<IEnumerable<AdminUserResponse>> GetAllUsersAsync(int skip, int take, CancellationToken ct = default)
        {
            var users = await _userRepo.GetAllAsync(ct);
            
            var userResponses = new List<AdminUserResponse>();
            
            foreach (var user in users.Skip(skip).Take(take))
            {
                var reportsCount = await _reportRepo.GetByUserIdAsync(user.Id, 0, int.MaxValue, ct);
                
                userResponses.Add(new AdminUserResponse(
                    user.Id,
                    user.Email,
                    user.DisplayName,
                    user.TrustPoints,
                    user.Badge,
                    user.Role,
                    user.IsEmailConfirmed,
                    null, // LastLogin would need to be tracked separately
                    reportsCount.Count,
                    DateTime.UtcNow // CreatedAt would need to be added to UserAccount
                ));
            }

            return userResponses;
        }

        public async Task<AdminUserResponse?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _userRepo.GetByIdAsync(userId, ct);
            if (user == null)
                return null;

            var reportsCount = await _reportRepo.GetByUserIdAsync(userId, 0, int.MaxValue, ct);

            return new AdminUserResponse(
                user.Id,
                user.Email,
                user.DisplayName,
                user.TrustPoints,
                user.Badge,
                user.Role,
                user.IsEmailConfirmed,
                null, // LastLogin would need to be tracked separately
                reportsCount.Count,
                DateTime.UtcNow // CreatedAt would need to be added to UserAccount
            );
        }

        public async Task<bool> UpdateUserAsync(Guid userId, UserUpdateRequest request, CancellationToken ct = default)
        {
            var user = await _userRepo.GetByIdAsync(userId, ct);
            if (user == null)
                return false;

            user.DisplayName = request.DisplayName;
            user.Role = request.Role;
            user.Badge = request.Badge;
            user.TrustPoints = request.TrustPoints;

            await _userRepo.UpdateAsync(user, ct);
            await _userRepo.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _userRepo.GetByIdAsync(userId, ct);
            if (user == null)
                return false;

            // In a real application, you might want to soft delete or handle related data
            // For now, we'll just remove the user
            // Note: This would need to be implemented in the repository
            return true; // Placeholder - actual implementation needed
        }

        public async Task<int> GetTotalUsersCountAsync(CancellationToken ct = default)
        {
            var users = await _userRepo.GetAllAsync(ct);
            return users.Count;
        }
    }
}
