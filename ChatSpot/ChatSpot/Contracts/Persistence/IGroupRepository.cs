using ChatSpot.Dtos.Responses;
using ChatSpot.Models.SQL;
using ChatSpot.ResourceParameters;

namespace ChatSpot.Contracts.Persistence;

public interface IGroupRepository
{
    Task<bool> UserInGroupAsync(Guid groupId, string userId);
    Task<Group?> GetByIdAsync(Guid id);
    Task<Group?> GetByIdWithMembersAsync(Guid id);
    Task<PagedResult<Group>> GetUserGroupsAsync(BaseResourceParameter baseResourceParameter , string userId);
    Task<Group> CreateAsync(Group group);
    Task UpdateAsync(Group group);
    Task<bool> AddMembersAsync(Guid groupId, List<string> userIds, string requesterId);
    Task<bool> RemoveMemberAsync(Guid groupId, string userId, string requesterId);
    Task<bool> UpdateMemberRoleAsync(Guid groupId, string userId, GroupRole role, string requesterId);
    Task<GroupMember?> GetMemberAsync(Guid groupId, string userId);
    
    Task<bool> IsGroupAdmin(Guid groupId, string userId);

    Task<bool> RemoveMemberAsync(Guid groupId, string userId);
}