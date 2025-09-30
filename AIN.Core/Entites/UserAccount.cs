using static AIN.Core.Enums.enums;

namespace AIN.Core.Entites
{
    public class UserAccount
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int TrustPoints { get; set; }
        public bool IsEmailConfirmed { get; set; } = false;
        public string? OtpCode { get; set; }
        public DateTime? OtpExpiryTime { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public TrustBadge Badge { get; set; } = TrustBadge.Newcomer;
        public UserRole Role { get; set; } = UserRole.User;
        public ICollection<Report> Reports { get; set; } = new List<Report>();

        
    }
}
