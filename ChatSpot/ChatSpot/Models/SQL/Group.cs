using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatSpot.Models.SQL;

public class Group
{
    [Key]
    public Guid GroupId { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = "";

    public string AvatarUrl { get; set; } = "";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    
    public string CreatorId { get; set; } = string.Empty;
    
    [ForeignKey("CreatedBy")]
    public ApplicationUser? Creator { get; set; }
}