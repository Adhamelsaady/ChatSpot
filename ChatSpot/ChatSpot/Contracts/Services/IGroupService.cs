using ChatSpot.Dtos;
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

    Task<PagedResult<MessageToReturnDto>> GetGroupMessages(BaseResourceParameter baseResourceParameter,
        string groupId, string currentUserId);

    Task<BaseResponse> DeleteMessageAsync(string messageId, string userId);
        Task<MessageToReturnDto> SendMessage(MessageForSending messageForSending, string currentUserId, Guid groupId);
}