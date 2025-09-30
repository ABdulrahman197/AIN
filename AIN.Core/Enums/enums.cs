using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIN.Core.Enums
{
    public class enums
    {
        public enum ReportCategory
        {
            Security = 1,
            PublicSafety = 2,
            Traffic = 3,
            Environment = 4,
            Other = 5
        }

        public enum ReportVisibility
        {
            Public = 1,
            Confidential = 2,
            Anonymous = 3
        }

        public enum ReportStatus
        {
            Pending = 1,
            InReview = 2,
            Dispatched = 3,
            Resolved = 4,
            Rejected = 5
        }

        public enum TrustBadge
        {
            Newcomer = 1,
            Contributor = 2,
            Trusted = 3,
            Guardian = 4,
            Vanguard = 5
        }

        public enum UserRole
        {
            User = 0,
            Admin = 1,
            Authority = 2
        }

    }
}
