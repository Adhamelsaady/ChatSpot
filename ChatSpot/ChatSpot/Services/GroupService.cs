using AutoMapper;
using ChatSpot.Contracts.Persistence;
using ChatSpot.Contracts.Services;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;
using ChatSpot.Dtos.Responses;
using ChatSpot.Hubs;
using ChatSpot.Models.SQL;
using ChatSpot.ResourceParameters;
using Microsoft.AspNetCore.SignalR;

namespace ChatSpot.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IMapper _mapper;
    private readonly IHubContext<ChatHub> _hub;
    private readonly IGroupMetaDataRepository _groupMetaDataRepository;

    public GroupService(IGroupRepository groupRepository, IMapper mapper, IHubContext<ChatHub> hub,
        IGroupMetaDataRepository groupMetaDataRepository)
    {
        _groupRepository = groupRepository;
        _mapper = mapper;
        _hub = hub;
        _groupMetaDataRepository = groupMetaDataRepository;
    }

    public async Task<GroupToReturnDto> CreateGroup(GroupToCreateDto createGroupDto, string currentUserId)
    {
        var allMemberIds = createGroupDto.Members.Distinct().Where(id => id != currentUserId).ToList();
        Console.WriteLine("1");
        var group = _mapper.Map<Group>(createGroupDto);
        group.CreatorId = currentUserId;
        group.CreatedAt = DateTime.UtcNow;
        var members = new List<GroupMember>
        {
            new()
            {
                GroupId = group.GroupId, UserId = currentUserId, Role = GroupRole.owner, JoinedAt = DateTime.UtcNow
            }
        };
        foreach (var uid in createGroupDto.Members.Distinct().Where(id => id != currentUserId))
            members.Add(new GroupMember()
                { GroupId = group.GroupId, UserId = uid, Role = GroupRole.member, JoinedAt = DateTime.UtcNow });

        Console.WriteLine("2");
        await _groupRepository.CreateAsync(group, members);
        await _groupMetaDataRepository.GetOrCreateAsync(group.GroupId);
        var fullGroup = await _groupRepository.GetByIdWithMembersAsync(group.GroupId);
        var groupToReturnDto = _mapper.Map<GroupToReturnDto>(fullGroup);
        Console.WriteLine("3");
        foreach (var uid in createGroupDto.Members.Where(id => id != currentUserId))
            await ChatHub.SendToUserAsync(_hub, uid, "AddedToGroup", groupToReturnDto);
        return groupToReturnDto;
    }

    public async Task<GroupToReturnDto> AddMembersToGroup(Guid groupId, GroupMemberToAddDto groupMemberToAddDto,
        string currentUserId)
    {
        var ok = await _groupRepository.AddMembersAsync(groupId, groupMemberToAddDto.UserIds, currentUserId);
        if (!ok) return new GroupToReturnDto() { IsSuccess = false, Message = "Something went wrong" };

        var group = await _groupRepository.GetByIdWithMembersAsync(groupId);
        var meta = await _groupMetaDataRepository.GetByGroupIdAsync(groupId.ToString());
        var groupToReturn = _mapper.Map<GroupToReturnDto>(group);
        foreach (var uid in groupMemberToAddDto.UserIds)
            await ChatHub.SendToUserAsync(_hub, uid, "AddedToGroup", groupToReturn);
        groupToReturn.IsSuccess = true;
        return groupToReturn;
    }

    public Task<PagedResult<GroupToReturnDto>> GetMyGroups(BaseResourceParameter baseResourceParameter, string currentUserId)
    {
        throw new NotImplementedException();
    }

}