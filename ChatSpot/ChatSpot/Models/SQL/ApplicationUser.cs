using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ChatSpot.Models.SQL;

public class ApplicationUser : IdentityUser
{
    [MaxLength(300)]
    public string Bio { get; set; } = "";

    public string ProfilePicture { get; set; } = string.Empty;

    [MaxLength(20)]
    public bool isOnline { get; set; } = false;

    public DateTime LastSeen { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? Otp { get; set; }
    
    public DateTime? OtpExpiry { get; set; }
    
    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
}