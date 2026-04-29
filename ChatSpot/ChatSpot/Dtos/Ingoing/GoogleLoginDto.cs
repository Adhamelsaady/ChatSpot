using System.ComponentModel.DataAnnotations;

namespace ChatSpot.Dtos.Ingoing;

public class GoogleLoginDto
{
    [Required]
    public string IdToken { get; set; } = null!;
}
