using System;

namespace AIN.Application.Dtos
{
    public record LikeResponse(
        Guid Id,
        Guid ReportId,
        Guid UserId,
        string UserDisplayName,
        DateTime CreatedAt
    );
}
