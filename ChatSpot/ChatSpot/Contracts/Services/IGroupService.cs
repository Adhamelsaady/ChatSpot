using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;
using ChatSpot.Dtos.Responses;
using ChatSpot.ResourceParameters;

namespace ChatSpot.Contracts.Services;

public interface IGroupService
{
    Task<GroupToReturnDto> CreateGroup(GroupToCreateDto createGroupDto, string currentUserId);
    Task<GroupToReturnDto> AddMembersToGroup(Guid groupId , GroupMemberToAddDto groupMemberToAddDto, string currentUserId);
    Task<PagedResult<GroupToReturnDto>> GetMyGroups(BaseResourceParameter baseResourceParameter , string currentUserId);
    Task<GroupToReturnDto> GetGroup(Guid groupId, string currentUserId);
}