using System.ComponentModel.DataAnnotations;

namespace AIN.Application.Dtos
{
    public class ProfileUpdateRequest
    {
        [Required]
        [MaxLength(128)]
        public string DisplayName { get; set; } = string.Empty;
    }
}


