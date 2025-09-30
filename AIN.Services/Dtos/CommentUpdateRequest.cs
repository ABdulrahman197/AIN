using System.ComponentModel.DataAnnotations;

namespace AIN.Application.Dtos
{
    public class CommentUpdateRequest
    {
        [Required]
        [MinLength(1)]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;
    }
}
