using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using static AIN.Core.Enums.enums;

namespace AIN.Core.Entites
{
    public class Report
    {
        public Guid Id { get; set; }
        public Guid? ReporterId { get; set; }
        public ReportVisibility Visibility { get; set; }
        public ReportCategory Category { get; set; }
        public ReportStatus Status { get; set; } = ReportStatus.Pending;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? RoutedAuthorityId { get; set; }
        public UserAccount? Reporter { get; set; }
        public Authority? RoutedAuthority { get; set; }
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
