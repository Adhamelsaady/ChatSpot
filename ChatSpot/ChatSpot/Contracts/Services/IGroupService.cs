using ChatSpot.Dtos;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;
using ChatSpot.Dtos.Responses;
using ChatSpot.ResourceParameters;

namespace ChatSpot.Contracts.Services;

public interface IGroupService
{
    Task<GroupToReturnDto> CreateGroup(GroupToCreateDto createGroupDto, string currentUserId);

    Task<GroupToReturnDto> AddMembersToGroup(Guid groupId, GroupMemberToAddDto groupMemberToAddDto,
        string currentUserId);

    Task<PagedResult<GroupToReturnDto>> GetMyGroups(BaseResourceParameter baseResourceParameter, string currentUserId);

    Task<PagedResult<MessageToReturnDto>> GetGroupMessages(BaseResourceParameter baseResourceParameter,
        string groupId, string currentUserId);

    Task<BaseResponse> RemoveMemberAsync(Guid groupId, string removerId, string targetUserId);
    Task<BaseResponse> DeleteMessageAsync(string messageId, string userId);
    Task<MessageToReturnDto> SendMessage(MessageForSending messageForSending, string currentUserId, Guid groupId);

    Task<BaseResponse> LeaveGroupAsync(Guid groupId, string userId);
    Task<BaseResponse> ToggleMemberAdminRoleAsync(Guid groupId, string targetUserId, string requesterId);

    Task<PagedResult<GroupMemberToReturnDto>> GetGroupMembers(
        BaseResourceParameter baseResourceParameter, Guid groupId, string currentUserId);
}