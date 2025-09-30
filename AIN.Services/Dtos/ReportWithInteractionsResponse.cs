using System;
using System.Collections.Generic;
using static AIN.Core.Enums.enums;

namespace AIN.Application.Dtos
{
    public record ReportWithInteractionsResponse(
        Guid Id,
        Guid? ReporterId,
        string? ReporterDisplayName,
        ReportVisibility Visibility,
        ReportCategory Category,
        ReportStatus Status,
        string Title,
        string Description,
        double Latitude,
        double Longitude,
        DateTimeOffset CreatedAt,
        Guid? RoutedAuthorityId,
        string? AuthorityName,
        IEnumerable<AttachmentResponse> Attachments,
        int LikeCount,
        int CommentCount,
        bool IsLikedByCurrentUser,
        IEnumerable<CommentResponse> RecentComments
    );
}
