using System.ComponentModel.DataAnnotations;

namespace ChatSpot.Dtos.Ingoing;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
