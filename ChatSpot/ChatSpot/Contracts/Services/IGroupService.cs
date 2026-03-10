using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;

namespace ChatSpot.Contracts.Services;

public interface IGroupService
{
    Task<GroupToReturnDto> CreateGroup(GroupToCreateDto createGroupDto, string currentUserId);
    Task<GroupToReturnDto> AddMembersToGroup(Guid groupId , GroupMemberToAddDto groupMemberToAddDto, string currentUserId);
}