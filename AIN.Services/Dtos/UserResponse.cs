using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AIN.Core.Enums.enums;

namespace AIN.Application.Dtos
{
    public record UserResponse
    (
        Guid Id,
        string Email,
        string DisplayName,
        int TrustPoints,
        TrustBadge Badge
    );
}
