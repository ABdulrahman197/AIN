using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AIN.Core.Enums.enums;

namespace AIN.Application.Dtos
{
    public record  ReportCreateRequest
    (
        string Title,
        string Description,
        ReportCategory Category,
        ReportVisibility Visibility,
        double Latitude,
        double Longitude,
        Guid? ReporterId
    );
    
}
