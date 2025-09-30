using System.ComponentModel.DataAnnotations;
using static AIN.Core.Enums.enums;

namespace AIN.Application.Dtos
{
    public class ReportUpdateRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public ReportCategory Category { get; set; }

        [Required]
        public ReportVisibility Visibility { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
    }
}
