namespace ChatSpot.Dtos.Outgoing;

public class MessageToReturnDto : BaseResponse
{
    public string? Id { get; set; }

    public string SenderId { get; set; } = string.Empty;        

    public string? ReceiverId { get; set; }                     

    public string? GroupId { get; set; }                        
    
    public string Content { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; } = false;
    
    public bool IsDeleted { get; set; } = false;

    public bool IsEdited { get; set; } = false;

    public DateTime? EditedAt { get; set; }
    
    public string? ReplyToId { get; set; }
    
    public string? ReplyToPreview { get; set; }
}