using System.ComponentModel.DataAnnotations;

namespace AIN.Application.Dtos
{
    public class CommentRequest
    {
        [Required]
        public Guid ReportId { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;
    }
}
