using System.ComponentModel.DataAnnotations;

namespace ChatSpot.Dtos.Ingoing;

public class LoginDto
{
    [Required]
    public string EmailOrUserName { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}