using AIN.Application.Dtos;
using AIN.Application.Interfaces.IRepos;
using AIN.Application.Interfaces.IServices;
using AIN.Core.Entites;
using System;
using System.Threading.Tasks;

namespace AIN.Application.Services
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepo _likeRepo;

        public LikeService(ILikeRepo likeRepo)
        {
            _likeRepo = likeRepo;
        }

        public async Task<bool> ToggleLikeAsync(Guid userId, LikeRequest request, CancellationToken ct = default)
        {
            var existingLike = await _likeRepo.GetByUserAndReportAsync(userId, request.ReportId, ct);
            
            if (existingLike != null)
            {
                // Unlike
                await _likeRepo.RemoveAsync(existingLike, ct);
                return false; // Like removed
            }
            else
            {
                // Like
                var newLike = new Like
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ReportId = request.ReportId,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _likeRepo.AddAsync(newLike, ct);
                await _likeRepo.SaveChangesAsync(ct);
                return true; // Like added
            }
        }

        public async Task<int> GetLikeCountAsync(Guid reportId, CancellationToken ct = default)
        {
            return await _likeRepo.GetCountByReportIdAsync(reportId, ct);
        }

        public async Task<bool> HasUserLikedAsync(Guid userId, Guid reportId, CancellationToken ct = default)
        {
            return await _likeRepo.HasUserLikedReportAsync(userId, reportId, ct);
        }
    }
}
