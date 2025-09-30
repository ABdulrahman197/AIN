using AIN.Application.Interfaces.IRepos;
using AIN.Application.Interfaces.IServices;
using AIN.Core.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AIN.Core.Enums.enums;

namespace AIN.Application.Services
{
    public class RoutingService : IRoutingService
    {
        private readonly IAuthorityRepo _authorities;

        public RoutingService(IAuthorityRepo authority)
        {
            _authorities = authority;
        }
        public async Task<Guid?> RouteToAuthorityIdAsync(Report report, CancellationToken ct = default)
        {
            string name;

            switch (report.Category)
            {
                case ReportCategory.Security:
                    name = "Police";
                    break;
                case ReportCategory.PublicSafety:
                    name = "Ambulance";
                    break;
                case ReportCategory.Traffic:
                    name = "Traffic Department";
                    break;
                case ReportCategory.Environment:
                    name = "Municipality";
                    break;
                default:
                    name = "General Authority";
                    break;
            }

            var authority = await _authorities.GetByNameAsync(name, ct);
            return authority?.Id;
        }

    }
}
