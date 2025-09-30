using AIN.Application.Interfaces.IRepos;
using AIN.Application.Interfaces.IServices;
using AIN.Core.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIN.Application.Services
{
    public class TrustPointsService : ITrustPointsService
    {
        private readonly IUserRepo _users;

        public TrustPointsService(IUserRepo users)
        {
            _users = users;
        }

        public async Task<int> AddPointsAsync(Guid userId, int delta, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(userId, ct);
            if (user == null) return 0;

            user.TrustPoints = Math.Max(0, user.TrustPoints + delta);
            user.Badge = TrustPointsRules.DetermineBadge(user.TrustPoints);

            await _users.SaveChangesAsync(ct);
            return user.TrustPoints;
        }

    }
}
