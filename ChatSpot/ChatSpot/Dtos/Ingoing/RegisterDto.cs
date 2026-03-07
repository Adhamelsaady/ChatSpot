using System.ComponentModel.DataAnnotations;

namespace ChatSpot.Dtos.Ingoing;

public class RegisterDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    [MinLength(2, ErrorMessage = "Full name must be at least 3 characters")]
    public string UserName { get; set; } = string.Empty;
    
    public string Bio { get; set; } = string.Empty;
}