using System;
using System.Collections.Generic;
using static AIN.Core.Enums.enums;

namespace AIN.Application.Dtos
{
    public record AdminUserResponse(
        Guid Id,
        string Email,
        string DisplayName,
        int TrustPoints,
        TrustBadge Badge,
        UserRole Role,
        bool IsEmailConfirmed,
        DateTime? LastLogin,
        int ReportsCount,
        DateTime CreatedAt
    );
}
