using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIN.Application.Interfaces.IServices
{
    public interface ITrustPointsService
    {
        Task<int> AddPointsAsync(Guid userId, int delta, CancellationToken ct = default);
    }
}
