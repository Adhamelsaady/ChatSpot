using Microsoft.EntityFrameworkCore;

namespace ChatSpot.Models.SQL;

public class ChatSpotDbContext : DbContext
{
    public ChatSpotDbContext(DbContextOptions<ChatSpotDbContext> options)
        : base(options)
    {
    }
    public DbSet<Group> Groups {get; set;}
    public DbSet<GroupMember> GroupMembers {get; set;}
    public DbSet<RefreshToken>  RefreshTokens {get; set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Group>(e =>
        {
            e.HasOne(g => g.Creator)
                .WithMany()
                .HasForeignKey(g => g.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GroupMember>(e =>
        {
            e.HasKey(gm => new { gm.GroupId, gm.UserId });
            e.HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(gm => gm.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}