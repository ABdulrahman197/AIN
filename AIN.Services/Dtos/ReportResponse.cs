using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AIN.Core.Enums.enums;

namespace AIN.Application.Dtos
{
    public record ReportResponse
    (
        Guid Id,
        string Title,
        string Description,
        ReportCategory Category,
        ReportVisibility Visibility,
        ReportStatus Status,
        double Latitude,
        double Longitude,
        DateTimeOffset CreatedAt,
        Guid? ReporterId,
        Guid? RoutedAuthorityId,
        IReadOnlyCollection<AttachmentResponse> Attachments
    );
}
