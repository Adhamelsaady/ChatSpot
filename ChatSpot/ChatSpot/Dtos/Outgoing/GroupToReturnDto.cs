namespace ChatSpot.Dtos.Outgoing;

public class GroupToReturnDto
{
    public Guid GroupId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; } = DateTime.Now;
    public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    public string LastMessage { get; set; } = string.Empty;
    public List<GroupMemberDto> GroupMemberDtos { get; set; } = new List<GroupMemberDto>();
}