using AIN.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIN.Application.Interfaces.IServices
{
    public interface IAdminService
    {
        Task<IEnumerable<AdminUserResponse>> GetAllUsersAsync(int skip, int take, CancellationToken ct = default);
        Task<AdminUserResponse?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
        Task<bool> UpdateUserAsync(Guid userId, UserUpdateRequest request, CancellationToken ct = default);
        Task<bool> DeleteUserAsync(Guid userId, CancellationToken ct = default);
        Task<int> GetTotalUsersCountAsync(CancellationToken ct = default);
    }
}
