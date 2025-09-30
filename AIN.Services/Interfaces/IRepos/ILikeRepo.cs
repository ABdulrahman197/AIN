using AIN.Core.Entites;
using System;
using System.Threading.Tasks;

namespace AIN.Application.Interfaces.IRepos
{
    public interface ILikeRepo
    {
        Task AddAsync(Like like, CancellationToken ct);
        Task RemoveAsync(Like like, CancellationToken ct);
        Task<Like?> GetByUserAndReportAsync(Guid userId, Guid reportId, CancellationToken ct);
        Task<int> GetCountByReportIdAsync(Guid reportId, CancellationToken ct);
        Task<bool> HasUserLikedReportAsync(Guid userId, Guid reportId, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
