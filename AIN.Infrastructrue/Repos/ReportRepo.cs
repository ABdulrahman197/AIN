using AIN.Application.Interfaces.IRepos;
using AIN.Core.Entites;
using AIN.Infrastructrue.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AIN.Core.Enums.enums;

namespace AIN.Infrastructrue.Repos
{
    public class ReportRepo : IReportRepo
    {
        private readonly AinDbContext _dbContext;

        public ReportRepo(AinDbContext dbContext)
        {
           _dbContext = dbContext;
        }

        public async  Task AddAsync(Report report, CancellationToken ct)
        {
            await _dbContext.Reports.AddAsync(report, ct);
        }

        public Task<Report?> GetByIdAsync(Guid id, bool includeAttachments, CancellationToken ct)
        {
            IQueryable<Report> query = _dbContext.Reports.AsQueryable();
            if (includeAttachments)
            {
                query = query.Include(r => r.Attachments);
            }
            return query.FirstOrDefaultAsync(r => r.Id == id, ct);
        }

        public Task<Report?> GetByIdWithInteractionsAsync(Guid id, Guid? currentUserId, CancellationToken ct)
        {
            return _dbContext.Reports
                .Include(r => r.Attachments)
                .Include(r => r.Likes)
                .Include(r => r.Comments.Where(c => !c.IsDeleted).OrderByDescending(c => c.CreatedAt).Take(5))
                    .ThenInclude(c => c.User)
                .Include(r => r.Reporter)
                .Include(r => r.RoutedAuthority)
                .FirstOrDefaultAsync(r => r.Id == id, ct);
        }

        public Task<IReadOnlyList<Report>> GetPublicFeedAsync(int skip, int take, CancellationToken ct)
        {
            return _dbContext.Reports
            .Where(r => r.Visibility == ReportVisibility.Public)
            .OrderByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Include(r => r.Attachments)
            .AsNoTracking()
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<Report>)t.Result, ct);
        }

        public Task<IReadOnlyList<Report>> GetPublicFeedWithInteractionsAsync(int skip, int take, Guid? currentUserId, CancellationToken ct)
        {
            return _dbContext.Reports
                .Where(r => r.Visibility == ReportVisibility.Public)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Include(r => r.Attachments)
                .Include(r => r.Likes)
                .Include(r => r.Comments.Where(c => !c.IsDeleted).OrderByDescending(c => c.CreatedAt).Take(3))
                    .ThenInclude(c => c.User)
                .Include(r => r.Reporter)
                .Include(r => r.RoutedAuthority)
                .AsNoTracking()
                .ToListAsync(ct)
                .ContinueWith(t => (IReadOnlyList<Report>)t.Result, ct);
        }

        public Task<IReadOnlyList<Report>> ListAllAsync(int skip, int take, CancellationToken ct)
        {
            return _dbContext.Reports
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Include(r => r.Attachments)
                .Include(r => r.Reporter)
                .Include(r => r.RoutedAuthority)
                .AsNoTracking()
                .ToListAsync(ct)
                .ContinueWith(t => (IReadOnlyList<Report>)t.Result, ct);
        }

        public Task<IReadOnlyList<Report>> ListByAuthorityAsync(Guid authorityId, int skip, int take, CancellationToken ct)
        {
            return _dbContext.Reports
                .Where(r => r.RoutedAuthorityId == authorityId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Include(r => r.Attachments)
                .Include(r => r.Reporter)
                .Include(r => r.RoutedAuthority)
                .AsNoTracking()
                .ToListAsync(ct)
                .ContinueWith(t => (IReadOnlyList<Report>)t.Result, ct);
        }

        public async Task UpdateAsync(Report report, CancellationToken ct)
        {
            _dbContext.Reports.Update(report);
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var report = await _dbContext.Reports.FindAsync(id);
            if (report != null)
            {
                _dbContext.Reports.Remove(report);
                await _dbContext.SaveChangesAsync(ct);
            }
        }

        public Task<IReadOnlyList<Report>> GetByUserIdAsync(Guid userId, int skip, int take, CancellationToken ct)
        {
            return _dbContext.Reports
                .Where(r => r.ReporterId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Include(r => r.Attachments)
                .AsNoTracking()
                .ToListAsync(ct)
                .ContinueWith(t => (IReadOnlyList<Report>)t.Result, ct);
        }

        public Task SaveChangesAsync(CancellationToken ct) => _dbContext.SaveChangesAsync(ct);

        public async Task<IEnumerable<Report>> GetReportsByUserIdAsync(Guid userId, CancellationToken ct) 
        {
            return await _dbContext.Reports
                .Where(r => r.ReporterId == userId)
                .ToListAsync(ct);
        }
    }
}
