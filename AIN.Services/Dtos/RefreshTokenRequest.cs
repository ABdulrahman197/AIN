using System.ComponentModel.DataAnnotations;

namespace AIN.Application.Dtos
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
