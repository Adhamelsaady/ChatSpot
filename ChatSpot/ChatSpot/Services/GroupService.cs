using AutoMapper;
using ChatSpot.Contracts.Persistence;
using ChatSpot.Contracts.Services;
using ChatSpot.Dtos;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;
using ChatSpot.Dtos.Responses;
using ChatSpot.Hubs;
using ChatSpot.Models.NoSQL;
using ChatSpot.Models.SQL;
using ChatSpot.ResourceParameters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatSpot.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IMapper _mapper;
    private readonly IHubContext<ChatHub> _hub;
    private readonly IGroupMetaDataRepository _groupMetaDataRepository;
    private readonly IMessageRepository _messageRepository;
    public GroupService(IGroupRepository groupRepository, IMapper mapper, IHubContext<ChatHub> hub,
        IGroupMetaDataRepository groupMetaDataRepository , IMessageRepository messageRepository)
    {
        _groupRepository = groupRepository;
        _mapper = mapper;
        _hub = hub;
        _groupMetaDataRepository = groupMetaDataRepository;
        _messageRepository = messageRepository;
    }

    public async Task<GroupToReturnDto> CreateGroup(GroupToCreateDto createGroupDto, string currentUserId)
    {
        createGroupDto.Members.Add(currentUserId);
        var group = _mapper.Map<Group>(createGroupDto);
        group.GroupId = Guid.NewGuid(); 
        group.CreatorId = currentUserId;
        group.CreatedAt = DateTime.UtcNow;
        var otherMemberIds = createGroupDto.Members
            .Distinct()
            .Where(id => id != currentUserId)
            .ToList();

      
        await _groupRepository.CreateAsync(group);
    
        await _groupMetaDataRepository.GetOrCreateAsync(group.GroupId.ToString());
        
        await _groupRepository.SetUserRole(group.GroupId, currentUserId , GroupRole.owner);
        
        var fullGroup = await _groupRepository.GetByIdWithMembersAsync(group.GroupId);
        var groupToReturnDto = _mapper.Map<GroupToReturnDto>(fullGroup);

        foreach (var memberId in otherMemberIds)
        {
            await _hub.Clients.User(memberId).SendAsync("AddedToGroup", groupToReturnDto);
        }

        return groupToReturnDto;
    }

    public async Task<GroupToReturnDto> AddMembersToGroup(Guid groupId, GroupMemberToAddDto groupMemberToAddDto,
        string currentUserId)
    {
        var ok = await _groupRepository.AddMembersAsync(groupId, groupMemberToAddDto.UserIds, currentUserId);
        if (!ok) return new GroupToReturnDto() { IsSuccess = false, Message = "Something went wrong" };
        var group = await _groupRepository.GetByIdWithMembersAsync(groupId);
        var groupToReturn = _mapper.Map<GroupToReturnDto>(group);
        groupToReturn.IsSuccess = true;
        foreach (var memberId in groupMemberToAddDto.UserIds)
            await _hub.Clients.User(memberId).SendAsync("AddedToGroup", groupToReturn);
        await _hub.Clients.Group($"group:{groupId}").SendAsync("MembersAdded", groupId, groupMemberToAddDto.UserIds);
        return groupToReturn;
    }

    public async Task<PagedResult<GroupToReturnDto>> GetMyGroups(BaseResourceParameter baseResourceParameter, string currentUserId)
    {
        var pagedGroups = await _groupRepository.GetUserGroupsAsync(baseResourceParameter, currentUserId);
        
        var groupIds = pagedGroups.Items.Select(g => g.GroupId.ToString()).ToList();
        
        var allMeta = await _groupMetaDataRepository.GetByGroupIdsAsync(groupIds);
        
        var metaDict = allMeta.ToDictionary(m => m.GroupId);
        
        var groupsToReturn = pagedGroups.Items.Select(group =>
        {
            var dto = _mapper.Map<GroupToReturnDto>(group);
            var groupIdStr = group.GroupId.ToString();
            if (metaDict.TryGetValue(groupIdStr, out var meta))
            {
                dto.LastMessage = meta.LastMessage;
                dto.LastUpdateTime = meta.LastUpdated;
                dto.UnreadCount = meta.UnreadCount?.GetValueOrDefault(currentUserId) ?? 0;
            }
            return dto;
        }).ToList();
        return new PagedResult<GroupToReturnDto>
        {
            Items = groupsToReturn,
            TotalCount = pagedGroups.TotalCount,
            PageNumber = pagedGroups.PageNumber,
            PageSize = pagedGroups.PageSize
        };
        
    }
    
    public async Task<MessageToReturnDto> SendMessage(MessageForSending messageForSending , string currentUserId , Guid groupId)
    {
        var messageDocument = _mapper.Map<MessageDocument>(messageForSending);
        
        string? replyPreview = null;
        if (!string.IsNullOrEmpty(messageForSending.ReplyToId))
        {
            var messageToReply = await _messageRepository.GetMessageByIdAsync(messageForSending.ReplyToId);
            if (messageToReply.IsDeleted) replyPreview = "Deleted Message";
            else replyPreview = messageToReply.Content[..Math.Min(60, messageToReply.Content.Length)];
        }
        messageDocument.GroupId = groupId.ToString();
        messageDocument.ReplyToPreview = replyPreview;
        messageDocument.SenderId = currentUserId;
        messageDocument.Timestamp = DateTime.UtcNow;
        var group = await _groupRepository.GetByIdWithMembersAsync(groupId);
        await _groupMetaDataRepository.UpsertAsync(groupId.ToString() ,  messageDocument.Content , currentUserId);
        var usersToUpdate = group.Members
            .Select(u => u.UserId)
            .Where(uid => uid != currentUserId)
            .ToList();
        await _groupMetaDataRepository.IncrementUnreadAsync(groupId.ToString(),usersToUpdate);
        var message = await _messageRepository.CreateMessageAsync(messageDocument);
        await _groupMetaDataRepository.UpdateLastMessage(message.GroupId, message.Id);
        var result = _mapper.Map<MessageToReturnDto>(message);
        result.IsSuccess = true;
        result.Message = "Message sent";
        await _hub.Clients.Group($"group:{groupId}").SendAsync("ReceiveGroupMessage", groupId.ToString(), result);
        return result;
    }
    public async Task<PagedResult<MessageToReturnDto>> GetGroupMessages(BaseResourceParameter baseResourceParameter,
        string groupId , string  currentUserId)
    {
        var messages = await _messageRepository.GetMessagesOfGroup(baseResourceParameter, groupId);
        await _groupMetaDataRepository.MarkGroupMessagesAsRead(groupId, currentUserId);
        PagedResult<MessageToReturnDto> messagesToReturn = new PagedResult<MessageToReturnDto>()
        {
            Items = _mapper.Map<List<MessageToReturnDto>>(messages.Items),
            TotalCount = messages.TotalCount,
            PageNumber = messages.PageNumber,
            PageSize =  messages.PageSize
        };
        return messagesToReturn;
    }
    
    public async Task<BaseResponse> DeleteMessageAsync(string messageId, string userId)
    {
        var message = await _messageRepository.GetMessageByIdAsync(messageId);
        var groupId = Guid.Parse(message.GroupId);
        if (message.SenderId != userId || !await _groupRepository.IsGroupAdmin(groupId, userId))
        {
            return new BaseResponse()
            {
                IsSuccess = false, Message = "UnAuthorized"
            };
        }

        if (message.IsDeleted == true)
        {
            return new BaseResponse()
            {
                IsSuccess = false, Message = "Already deleted"
            };
        }

        await _messageRepository.DeleteMessageAsync(messageId);
        await _hub.Clients.Group($"group:{groupId}").SendAsync("GroupMessageDeleted", message.GroupId, messageId);
        return  new BaseResponse() {IsSuccess = true, Message = "Deleted Successfully"};
    }

    public async Task<BaseResponse> RemoveMemberAsync(Guid groupId, string removerId, string targetUserId)
    {
        var admin = await _groupRepository.GetMemberAsync(groupId, removerId);
        var target = await _groupRepository.GetMemberAsync(groupId, targetUserId);
        if (admin == null || target == null)
        {
            return new BaseResponse() {IsSuccess = false, Message = "UnAuthorized"};
        }
        if (admin.Role >= target.Role && admin.Role != GroupRole.owner)
        {
            return new BaseResponse() {IsSuccess = false, Message = "UnAuthorized"};
        }
        await _groupRepository.RemoveMemberAsync(groupId, targetUserId);
        await _hub.Clients.User(targetUserId).SendAsync("RemovedFromGroup", groupId);
        await _hub.Clients.Group($"group:{groupId}").SendAsync("MemberRemoved", groupId, targetUserId);
        return new BaseResponse() {IsSuccess = true, Message = "Removed Successfully"};
    }


    public async Task<BaseResponse> LeaveGroupAsync(Guid groupId, string userId)
    {
        var member = await _groupRepository.GetMemberAsync(groupId, userId);
        if(member.Role == GroupRole.owner) 
            return  new BaseResponse(){IsSuccess = false, Message = "Owner can't leave the group"};
        var removed = await _groupRepository.RemoveMemberAsync(groupId, userId);
        if (removed)
        {
            await _hub.Clients.Group($"group:{groupId}").SendAsync("MemberLeft", groupId, userId);
            return new BaseResponse(){IsSuccess = true, Message = "Removed Successfully"};
        }
        else
        {
            return new BaseResponse() {IsSuccess = false, Message = "Member can't be removed"};
        }
    }
    
    public async Task<BaseResponse> ToggleMemberAdminRoleAsync(Guid groupId, string targetUserId, string requesterId)
    {
        var targetMember = await _groupRepository.GetMemberAsync(groupId, targetUserId);
        if (targetMember == null)
            return new BaseResponse { IsSuccess = false, Message = "User is not in this group." };
        if (targetMember.Role == GroupRole.owner)
            return new BaseResponse { IsSuccess = false, Message = "Cannot change the role of the group owner." };
        var newRole = targetMember.Role == GroupRole.admin ? GroupRole.member : GroupRole.admin;
        var success = await _groupRepository.UpdateMemberRoleAsync(groupId, targetUserId, newRole, requesterId);
        if (!success)
        {
            return new BaseResponse { 
                IsSuccess = false, 
                Message = "Action failed. Ensure you have the required permissions (Admin/Owner)." 
            };
        }
        await _hub.Clients.User(targetUserId).SendAsync("RoleChanged", groupId, newRole.ToString());
        await _hub.Clients.Group($"group:{groupId}").SendAsync("MemberRoleChanged", groupId, targetUserId, newRole.ToString());
        return new BaseResponse { 
            IsSuccess = true, 
            Message = $"User successfully changed to {newRole}." 
        };
    }
    
    public async Task<PagedResult<GroupMemberToReturnDto>> GetGroupMembers(
        BaseResourceParameter baseResourceParameter, Guid groupId, string currentUserId)
    {
        var group = await _groupRepository.GetByIdWithMembersAsync(groupId);
        if (group == null || !group.Members.Any(m => m.UserId == currentUserId))
            return null;
        var query = group.Members.AsQueryable();
        var totalCount = query.Count();
        var items = query
            .Skip((baseResourceParameter.PageNumber - 1) * baseResourceParameter.PageSize)
            .Take(baseResourceParameter.PageSize)
            .ToList();
        return new PagedResult<GroupMemberToReturnDto>
        {
            Items = _mapper.Map<List<GroupMemberToReturnDto>>(items),
            TotalCount = totalCount,
            PageNumber = baseResourceParameter.PageNumber,
            PageSize = baseResourceParameter.PageSize
        };
    }
}