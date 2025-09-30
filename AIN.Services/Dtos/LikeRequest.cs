using System.ComponentModel.DataAnnotations;

namespace AIN.Application.Dtos
{
    public class LikeRequest
    {
        [Required]
        public Guid ReportId { get; set; }
    }
}
