using ChatSpot.Models.SQL;

namespace ChatSpot.Dtos.Outgoing;

public class GroupMemberDto : BaseResponse
{
    public GroupRole Role { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    public string UserId { get; set; } = string.Empty;
    
}