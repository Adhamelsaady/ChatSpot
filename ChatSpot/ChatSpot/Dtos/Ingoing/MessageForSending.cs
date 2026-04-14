using Microsoft.AspNetCore.Http;

namespace ChatSpot.Dtos.Ingoing;

public class MessageForSending
{
    public string? Content { get; set; } = string.Empty;
    public string? ReplyToId { get; set; } = string.Empty;
    
    public IFormFile? Media { get; set; }
}