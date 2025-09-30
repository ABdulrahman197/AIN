using AIN.Application.Interfaces.IRepos;
using AIN.Core.Entites;
using AIN.Infrastructrue.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIN.Infrastructrue.Repos
{
    public class CommentRepo : ICommentRepo
    {
        private readonly AinDbContext _db;

        public CommentRepo(AinDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Comment comment, CancellationToken ct)
        {
            await _db.Comments.AddAsync(comment, ct);
        }

        public Task<Comment?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return _db.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
        }

        public Task<IEnumerable<Comment>> GetByReportIdAsync(Guid reportId, int skip, int take, CancellationToken ct)
        {
            return _db.Comments
                .Where(c => c.ReportId == reportId && !c.IsDeleted)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct)
                .ContinueWith(t => (IEnumerable<Comment>)t.Result, ct);
        }

        public Task<int> GetCountByReportIdAsync(Guid reportId, CancellationToken ct)
        {
            return _db.Comments
                .CountAsync(c => c.ReportId == reportId && !c.IsDeleted, ct);
        }

        public async Task UpdateAsync(Comment comment, CancellationToken ct)
        {
            comment.UpdatedAt = DateTime.UtcNow;
            _db.Comments.Update(comment);
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var comment = await _db.Comments.FindAsync(id);
            if (comment != null)
            {
                comment.IsDeleted = true;
                comment.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
            }
        }

        public Task SaveChangesAsync(CancellationToken ct)
        {
            return _db.SaveChangesAsync(ct);
        }
    }
}
