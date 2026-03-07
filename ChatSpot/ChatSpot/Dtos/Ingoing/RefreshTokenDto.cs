using System.ComponentModel.DataAnnotations;

namespace ChatSpot.Dtos.Ingoing;

public class RefreshTokenDto
{
    [Required]
    public string Token { get; set; }
    
    [Required]
    public string RefreshToken { get; set; }
}