using AIN.Core.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIN.Application.Interfaces.IRepos
{
    public interface IReportRepo
    {
        Task AddAsync(Report report, CancellationToken ct);
        Task<Report?> GetByIdAsync(Guid id, bool includeAttachments, CancellationToken ct);
        Task<Report?> GetByIdWithInteractionsAsync(Guid id, Guid? currentUserId, CancellationToken ct);
        Task<IReadOnlyList<Report>> GetPublicFeedAsync(int skip, int take, CancellationToken ct);
        Task<IEnumerable<Report>> GetReportsByUserIdAsync (Guid userId, CancellationToken ct);   //For Profike 
        Task<IReadOnlyList<Report>> GetPublicFeedWithInteractionsAsync(int skip, int take, Guid? currentUserId, CancellationToken ct);
        Task<IReadOnlyList<Report>> ListAllAsync(int skip, int take, CancellationToken ct);
        Task<IReadOnlyList<Report>> ListByAuthorityAsync(Guid authorityId, int skip, int take, CancellationToken ct);
        Task UpdateAsync(Report report, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
        Task<IReadOnlyList<Report>> GetByUserIdAsync(Guid userId, int skip, int take, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
