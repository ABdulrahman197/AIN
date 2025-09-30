using AIN.Core.Entites;
using AIN.Core.Enums;
using static AIN.Core.Enums.enums;

namespace AIN.Api.Models
{
    public class AuthorityDashboardViewModel
    {
        public IEnumerable<Report> Reports { get; set; } = new List<Report>();
        public ReportStatusSummary Summary { get; set; } = new ReportStatusSummary();
        public ReportFilter Filter { get; set; } = new ReportFilter();
    }

    public class ReportStatusSummary
    {
        public int TotalReports { get; set; }
        public int PendingCount { get; set; }
        public int InReviewCount { get; set; }
        public int DispatchedCount { get; set; }
        public int ResolvedCount { get; set; }
        public int RejectedCount { get; set; }
    }

    public class ReportFilter
    {
        public string? SearchTerm { get; set; }
        public ReportStatus? StatusFilter { get; set; }
        public ReportCategory? CategoryFilter { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
