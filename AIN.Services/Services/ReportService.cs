using AIN.Application.Dtos;
using AIN.Application.Interfaces.IRepos;
using AIN.Application.Interfaces.IServices;
using AIN.Application.Services;
using AIN.Core.Entites;
using AIN.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AIN.Core.Enums.enums;

namespace AIN.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepo _reports;
        private readonly IRoutingService _routingService;
        private readonly ITrustPointsService _trustPointsService;

        public ReportService(IReportRepo reports, IRoutingService routingService, ITrustPointsService trustPointsService)
        {
            _reports = reports;
            _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
            _trustPointsService = trustPointsService ?? throw new ArgumentNullException(nameof(trustPointsService));
        }

        public async Task<Report> CreateAsync(ReportCreateRequest request, CancellationToken ct = default)
        {
            var entity = new Report                    //Map From Dto To Report Entity For Saving In DB
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Category = request.Category,
                Visibility = request.Visibility,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                ReporterId = request.Visibility == ReportVisibility.Anonymous ? null : request.ReporterId,
                Status = ReportStatus.Pending
            };

            entity.RoutedAuthorityId = await _routingService.RouteToAuthorityIdAsync(entity, ct);
            await _reports.AddAsync(entity, ct);
            await _reports.SaveChangesAsync(ct);
            return entity;

        }

        public Task<Report?> GetAsync(Guid id, CancellationToken ct = default)
        {
            return _reports.GetByIdAsync(id, includeAttachments: true, ct);
        }

        public async Task<ReportWithInteractionsResponse?> GetWithInteractionsAsync(Guid id, Guid? currentUserId, CancellationToken ct = default)
        {
            var report = await _reports.GetByIdWithInteractionsAsync(id, currentUserId, ct);
            if (report == null) return null;

            return MapToReportWithInteractionsResponse(report, currentUserId);
        }

        public async Task<IReadOnlyList<Report>> GetPublicFeedAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var skip = Math.Max(0, (page - 1) * pageSize);
            var list = await _reports.GetPublicFeedAsync(skip, pageSize, ct);
            return list;
        }

        public async Task<IEnumerable<ReportWithInteractionsResponse>> GetPublicFeedWithInteractionsAsync(int page, int pageSize, Guid? currentUserId, CancellationToken ct = default)
        {
            var skip = Math.Max(0, (page - 1) * pageSize);
            var reports = await _reports.GetPublicFeedWithInteractionsAsync(skip, pageSize, currentUserId, ct);
            
            return reports.Select(r => MapToReportWithInteractionsResponse(r, currentUserId));
        }

        public async Task<IReadOnlyList<Report>> ListAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var skip = Math.Max(0, (page - 1) * pageSize);
            return await _reports.ListAllAsync(skip, pageSize, ct);
        }

        public async Task<IReadOnlyList<Report>> ListByAuthorityAsync(Guid authorityId, int page, int pageSize, CancellationToken ct = default)
        {
            var skip = Math.Max(0, (page - 1) * pageSize);
            return await _reports.ListByAuthorityAsync(authorityId, skip, pageSize, ct);
        }

        public async Task<Report?> UpdateAsync(Guid id, ReportUpdateRequest request, Guid userId, CancellationToken ct = default)
        {
            var report = await _reports.GetByIdAsync(id, includeAttachments: false, ct);
            if (report == null || report.ReporterId != userId) return null;

            report.Title = request.Title;
            report.Description = request.Description;
            report.Category = request.Category;
            report.Visibility = request.Visibility;
            report.Latitude = request.Latitude;
            report.Longitude = request.Longitude;

            await _reports.UpdateAsync(report, ct);
            return report;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
        {
            var report = await _reports.GetByIdAsync(id, includeAttachments: false, ct);
            if (report == null || report.ReporterId != userId) return false;

            await _reports.DeleteAsync(id, ct);
            return true;
        }

        public async Task<IEnumerable<Report>> GetUserReportsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
        {
            var skip = Math.Max(0, (page - 1) * pageSize);
            var reports = await _reports.GetByUserIdAsync(userId, skip, pageSize, ct);
            return reports;
        }

        public async Task UpdateStatusAsync(Guid id, enums.ReportStatus status, CancellationToken ct = default)
        {
            var entity = await _reports.GetByIdAsync(id, includeAttachments: false, ct);
            if (entity == null) return;
            entity.Status = status;
            if (entity.ReporterId.HasValue)
            {
                int delta = 0;
                if (status == ReportStatus.Resolved) delta = 10;
                else if (status == ReportStatus.Rejected) delta = -5;
                if (delta != 0)
                {
                    await _trustPointsService.AddPointsAsync(entity.ReporterId.Value, delta, ct);
                }
            }
            await _reports.SaveChangesAsync(ct);
        }

        private static ReportWithInteractionsResponse MapToReportWithInteractionsResponse(Report report, Guid? currentUserId)
        {
            var likeCount = report.Likes?.Count ?? 0;
            var commentCount = report.Comments?.Count(c => !c.IsDeleted) ?? 0;
            var isLikedByCurrentUser = currentUserId.HasValue && 
                report.Likes?.Any(l => l.UserId == currentUserId.Value) == true;

            var recentComments = report.Comments?
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => new CommentResponse(
                    c.Id,
                    c.ReportId,
                    c.UserId,
                    c.User?.DisplayName ?? "Unknown User",
                    c.Content,
                    c.CreatedAt,
                    c.UpdatedAt
                )) ?? Enumerable.Empty<CommentResponse>();

            return new ReportWithInteractionsResponse(
                report.Id,
                report.ReporterId,
                report.Reporter?.DisplayName,
                report.Visibility,
                report.Category,
                report.Status,
                report.Title,
                report.Description,
                report.Latitude,
                report.Longitude,
                report.CreatedAt,
                report.RoutedAuthorityId,
                report.RoutedAuthority?.Name,
                report.Attachments?.Select(a => new AttachmentResponse(
                    a.Id,
                    a.FileName,
                    a.ContentType,
                    a.SizeBytes,
                    $"/uploads/{System.IO.Path.GetFileName(a.StoragePath)}"
                )) ?? Enumerable.Empty<AttachmentResponse>(),
                likeCount,
                commentCount,
                isLikedByCurrentUser,
                recentComments
            );
        }
    }
}




