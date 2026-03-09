using AutoMapper;
using ChatSpot.Contracts.Persistence;
using ChatSpot.Contracts.Services;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;
using ChatSpot.Hubs;
using ChatSpot.Models.SQL;
using Microsoft.AspNetCore.SignalR;

namespace ChatSpot.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IMapper _mapper;
    private readonly IHubContext<ChatHub> _hub;
    public GroupService(IGroupRepository groupRepository , IMapper mapper)
    {
        _groupRepository = groupRepository;
        _mapper = mapper;
    }

    public async Task<GroupToReturnDto> CreateGroup(GroupToCreateDto createGroupDto , string currentUserId)
    {
        var allMemberIds = createGroupDto.Members.Distinct().Where(id => id != currentUserId).ToList();
        createGroupDto.Members.Add(currentUserId);
        var members = _mapper.Map<List<GroupMember>>(createGroupDto.Members);
        var group = _mapper.Map<Group>(createGroupDto);
        await _groupRepository.CreateAsync(group , members);
        // add to the groupmeta 
        var fullGroup = await _groupRepository.GetByIdWithMembersAsync(group.GroupId);
        var groupToReturnDto = _mapper.Map<GroupToReturnDto>(fullGroup);
        foreach (var uid in createGroupDto.Members.Where(id => id != currentUserId))
            await ChatHub.SendToUserAsync(_hub, uid, "AddedToGroup", groupToReturnDto);
        return groupToReturnDto;
    }
    
}