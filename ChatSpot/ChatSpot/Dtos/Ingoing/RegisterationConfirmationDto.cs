using System.ComponentModel.DataAnnotations;

namespace ChatSpot.Dtos.Ingoing;

public class RegisterationConfirmationDto
{
    [Required]
    public string Otp { get; set; } = string.Empty;
    [Required]
    public string Email { get; set; } = string.Empty;
}