using ChatSpot.Models.SQL;

namespace ChatSpot.Contracts.Persistence;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(Guid id);
    Task<Group?> GetByIdWithMembersAsync(Guid id);
    Task<List<Group>> GetUserGroupsAsync(string userId);
    Task<Group> CreateAsync(Group group, List<GroupMember> members);
    Task UpdateAsync(Group group);
    Task<bool> AddMembersAsync(Guid groupId, List<string> userIds, string requesterId);
    Task<bool> RemoveMemberAsync(Guid groupId, string userId, string requesterId);
    Task<bool> UpdateMemberRoleAsync(Guid groupId, string userId, GroupRole role, string requesterId);
    Task<GroupMember?> GetMemberAsync(Guid groupId, string userId);
}