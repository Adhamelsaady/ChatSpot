using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;

namespace ChatSpot.Contracts.Services;

public interface IGroupService
{
    Task<GroupToReturnDto> CreateGroup(GroupToCreateDto createGroupDto, string currentUserId);
}