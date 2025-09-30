using System.ComponentModel.DataAnnotations;
using static AIN.Core.Enums.enums;

namespace AIN.Application.Dtos
{
    public class UserUpdateRequest
    {
        [Required]
        [MaxLength(128)]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        [Required]
        public TrustBadge Badge { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int TrustPoints { get; set; }
    }
}
