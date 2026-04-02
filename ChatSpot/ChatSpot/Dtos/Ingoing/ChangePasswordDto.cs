using System.ComponentModel.DataAnnotations;

namespace ChatSpot.Dtos.Ingoing;

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}
