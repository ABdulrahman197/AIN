using AIN.Core.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIN.Application.Interfaces.IServices
{
    public interface IRoutingService
    {
        Task<Guid?> RouteToAuthorityIdAsync(Report report, CancellationToken ct = default);
    }
}
