using System.ComponentModel.DataAnnotations;

namespace AIN.Application.Dtos
{
    public class ForgetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
