using System;

namespace AIN.Core.Entites
{
    public class Like
    {
        public Guid Id { get; set; }
        public Guid ReportId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Report? Report { get; set; }
        public UserAccount? User { get; set; }
    }
}
