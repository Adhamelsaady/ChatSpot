namespace ChatSpot.Dtos.Outgoing;

public class GroupToReturnDto : BaseResponse
{
    public Guid GroupId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    public string LastMessage { get; set; } = string.Empty;
    public int UnreadCount { get; set; } = 0;
    public List<GroupMemberDto> GroupMemberDtos { get; set; } = new List<GroupMemberDto>();
}