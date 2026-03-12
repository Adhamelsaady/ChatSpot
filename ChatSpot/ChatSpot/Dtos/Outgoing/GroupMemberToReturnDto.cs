using ChatSpot.Models.SQL;

namespace ChatSpot.Dtos.Outgoing;

public class GroupMemberToReturnDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public GroupRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}