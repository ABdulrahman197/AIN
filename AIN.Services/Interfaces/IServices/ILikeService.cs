using AIN.Application.Dtos;
using System;
using System.Threading.Tasks;

namespace AIN.Application.Interfaces.IServices
{
    public interface ILikeService
    {
        Task<bool> ToggleLikeAsync(Guid userId, LikeRequest request, CancellationToken ct = default);
        Task<int> GetLikeCountAsync(Guid reportId, CancellationToken ct = default);
        Task<bool> HasUserLikedAsync(Guid userId, Guid reportId, CancellationToken ct = default);
    }
}
