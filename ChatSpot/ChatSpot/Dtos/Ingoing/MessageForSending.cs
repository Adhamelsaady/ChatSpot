using System.ComponentModel.DataAnnotations;

namespace ChatSpot.Dtos.Ingoing;

public class MessageForSending
{
    [Required]
    public string Content { get; set; }
    public string? GroupId { get; set; }
    public string? ReceiverId { get; set; }
    public string? ReplyToId { get; set; } = string.Empty;
}