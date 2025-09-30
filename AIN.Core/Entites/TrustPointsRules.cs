using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AIN.Core.Enums.enums;

namespace AIN.Core.Entites
{
    public static class TrustPointsRules
    {
        public static TrustBadge DetermineBadge(int trustPoints)
        {
            // Arabic levels mapping:
            // 0-49  454F282A2F26 (Newcomer)
            // 50-99 454F3327475045 (Contributor)
            // 100-199 454F482B4F4842 (Trusted)
            // 200-499 2D273133 (Guardian)
            // 500+ 3127262F (Vanguard)

            if (trustPoints >= 500) return TrustBadge.Vanguard;
            if (trustPoints >= 200) return TrustBadge.Guardian;
            if (trustPoints >= 100) return TrustBadge.Trusted;
            if (trustPoints >= 50) return TrustBadge.Contributor;
            return TrustBadge.Newcomer;
        }
    }
}
