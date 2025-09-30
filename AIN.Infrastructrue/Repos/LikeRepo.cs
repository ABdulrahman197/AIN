using AIN.Application.Interfaces.IRepos;
using AIN.Core.Entites;
using AIN.Infrastructrue.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AIN.Infrastructrue.Repos
{
    public class LikeRepo : ILikeRepo
    {
        private readonly AinDbContext _db;

        public LikeRepo(AinDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Like like, CancellationToken ct)
        {
            await _db.Likes.AddAsync(like, ct);
        }

        public async Task RemoveAsync(Like like, CancellationToken ct)
        {
            _db.Likes.Remove(like);
            await _db.SaveChangesAsync(ct);
        }

        public Task<Like?> GetByUserAndReportAsync(Guid userId, Guid reportId, CancellationToken ct)
        {
            return _db.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.ReportId == reportId, ct);
        }

        public Task<int> GetCountByReportIdAsync(Guid reportId, CancellationToken ct)
        {
            return _db.Likes
                .CountAsync(l => l.ReportId == reportId, ct);
        }

        public Task<bool> HasUserLikedReportAsync(Guid userId, Guid reportId, CancellationToken ct)
        {
            return _db.Likes
                .AnyAsync(l => l.UserId == userId && l.ReportId == reportId, ct);
        }

        public Task SaveChangesAsync(CancellationToken ct)
        {
            return _db.SaveChangesAsync(ct);
        }
    }
}
