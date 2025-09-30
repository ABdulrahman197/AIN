using System;

namespace AIN.Application.Dtos
{
    public record CommentResponse(
        Guid Id,
        Guid ReportId,
        Guid UserId,
        string UserDisplayName,
        string Content,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
