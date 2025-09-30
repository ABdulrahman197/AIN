using AIN.Core.Entites;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIN.Application.Interfaces.IRepos
{
    public interface ICommentRepo
    {
        Task AddAsync(Comment comment, CancellationToken ct);
        Task<Comment?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<Comment>> GetByReportIdAsync(Guid reportId, int skip, int take, CancellationToken ct);
        Task<int> GetCountByReportIdAsync(Guid reportId, CancellationToken ct);
        Task UpdateAsync(Comment comment, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
