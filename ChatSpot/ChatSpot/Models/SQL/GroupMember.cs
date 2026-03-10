using System.ComponentModel.DataAnnotations;

namespace ChatSpot.Models.SQL;

public class GroupMember
{
    public GroupRole Role { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Guid GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Group Group { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}

public enum GroupRole
{
    owner,
    admin,
    member
}