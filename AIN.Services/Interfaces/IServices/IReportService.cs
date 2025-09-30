using AIN.Application.Dtos;
using AIN.Core.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AIN.Core.Enums.enums;

namespace AIN.Application.Interfaces.IServices
{
    public interface IReportService
    {
        Task<Report> CreateAsync(ReportCreateRequest request, CancellationToken ct = default);
        Task<Report?> GetAsync(Guid id, CancellationToken ct = default);
        Task<ReportWithInteractionsResponse?> GetWithInteractionsAsync(Guid id, Guid? currentUserId, CancellationToken ct = default);
        Task<IReadOnlyList<Report>> GetPublicFeedAsync(int page, int pageSize, CancellationToken ct = default);
        Task<IEnumerable<ReportWithInteractionsResponse>> GetPublicFeedWithInteractionsAsync(int page, int pageSize, Guid? currentUserId, CancellationToken ct = default);
        Task<IReadOnlyList<Report>> ListAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<IReadOnlyList<Report>> ListByAuthorityAsync(Guid authorityId, int page, int pageSize, CancellationToken ct = default);
        Task<Report?> UpdateAsync(Guid id, ReportUpdateRequest request, Guid userId, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
        Task<IEnumerable<Report>> GetUserReportsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
        Task UpdateStatusAsync(Guid id, ReportStatus status, CancellationToken ct = default);
    }
}
