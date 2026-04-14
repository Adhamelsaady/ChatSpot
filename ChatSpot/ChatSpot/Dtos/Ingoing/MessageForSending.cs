using System.ComponentModel.DataAnnotations;

namespace ChatSpot.Dtos.Ingoing;

public class MessageForSending
{
    [Required]
    public string Content { get; set; } 
    public string? ReplyToId { get; set; } = string.Empty;
    
    public IFormFile? Media { get; set; }
}