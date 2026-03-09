using ChatSpot.Contracts.Persistence;
using ChatSpot.Models.SQL;
using Microsoft.EntityFrameworkCore;

namespace ChatSpot.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly ChatSpotDbContext _db;
    public GroupRepository(ChatSpotDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<Group?> GetByIdAsync(Guid id)
    { 
        return await _db.Groups.FindAsync(id);
    }

    public async Task<Group?> GetByIdWithMembersAsync(Guid id)
    {
        return await _db.Groups
            .Include(g => g.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.GroupId == id);
    }

    public async Task<List<Group>> GetUserGroupsAsync(string userId)
    {
        return await _db.Groups
            .Include(g => g.Members)
            .ThenInclude(m => m.User)
            .Where(g => g.Members.Any(m => m.UserId == userId))
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }
    public async Task<Group> CreateAsync(Group group, List<GroupMember> members)
    {
        _db.Groups.Add(group);
        _db.GroupMembers.AddRange(members);
        await _db.SaveChangesAsync();
        return group;
    }
    public async Task UpdateAsync(Group group)
    {
        _db.Groups.Update(group);
        await _db.SaveChangesAsync();
    }
    
    
    public async Task<bool> AddMembersAsync(Guid groupId, List<string> userIds, string requesterId)
    {
        var requester = await GetMemberAsync(groupId, requesterId);
        if (requester == null || requester.Role == GroupRole.member) return false;

        var existing = await _db.GroupMembers
            .Where(m => m.GroupId == groupId && userIds.Contains(m.UserId))
            .Select(m => m.UserId)
            .ToListAsync();

        var newMembers = userIds
            .Except(existing)
            .Select(uid => new GroupMember
            {
                GroupId = groupId, UserId = uid, Role = GroupRole.member, JoinedAt = DateTime.UtcNow
            });

        _db.GroupMembers.AddRange(newMembers);
        await _db.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> RemoveMemberAsync(Guid groupId, string userId, string requesterId)
    {

        if (userId != requesterId)
        {
            var requester = await GetMemberAsync(groupId, requesterId);
            if (requester == null || requester.Role == GroupRole.member) return false;
        }
        await _db.GroupMembers
            .Where(m => m.GroupId == groupId && m.UserId == userId)
            .ExecuteDeleteAsync();
        return true;
    }
    
    public async Task<bool> UpdateMemberRoleAsync(Guid groupId, string userId, GroupRole role, string requesterId)
    {
        var requester = await GetMemberAsync(groupId, requesterId);
        var applyOn = await GetMemberAsync(groupId, userId);
        if (requester?.Role != GroupRole.admin || applyOn?.Role == GroupRole.owner) return false;
        await _db.GroupMembers
            .Where(m => m.GroupId == groupId && m.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.Role, role));

        return true;
    }

    public async Task<GroupMember?> GetMemberAsync(Guid groupId, string userId)
    {
       return await _db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);
    }
}