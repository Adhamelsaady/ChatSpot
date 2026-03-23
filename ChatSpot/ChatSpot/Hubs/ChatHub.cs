using System.Security.Claims;
using ChatSpot.Contracts.Services;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatSpot.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IConnectionService _connectionService;

    public ChatHub(IConnectionService connectionService)
    {
        _connectionService = connectionService;
    }
    
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        await _connectionService.ConnectAsync(userId, Context.ConnectionId);
        await Clients.Others.SendAsync("UserOnline", userId);
        await base.OnConnectedAsync();
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        await _connectionService.DisconnectAsync(Context.ConnectionId);
        var remaining = await _connectionService.GetUserConnectionsAsync(userId);
        if (!remaining.Any())
            await Clients.Others.SendAsync("UserOffline", userId);
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task SendDirectMessage(string conversationId, MessageForSending messageForSending , MessageToReturnDto messageToReturnDto)
    {
        var senderId =  Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!messageToReturnDto.IsSuccess) return;

        await Clients.User(senderId).SendAsync("ReceiveDirectMessage", conversationId, messageToReturnDto);
        await Clients.User(messageToReturnDto.ReceiverId).SendAsync("ReceiveDirectMessage", conversationId, messageToReturnDto);
    }
    public async Task MarkConversationRead(string conversationId, string otherUserId)
    {
        await Clients.User(otherUserId)
            .SendAsync("ConversationRead", conversationId, Context.User?.FindFirstValue(ClaimTypes.NameIdentifier));
    }

    public async Task JoinGroup(string groupId)
    { 
        await Groups.AddToGroupAsync(Context.ConnectionId, $"group:{groupId}");
    }

    public async Task LeaveGroup(string groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group:{groupId}");
    }
    
    public async Task SendGroupMessage(string groupId, MessageForSending messageForSending , MessageToReturnDto messageToReturnDto)
    {
        if (!Guid.TryParse(groupId, out var groupGuid)) return;
        if (!messageToReturnDto.IsSuccess) return;
        await Clients.Group($"group:{groupId}")
            .SendAsync("ReceiveGroupMessage", groupId, messageToReturnDto);
    }
    public async Task MarkGroupRead(string groupId)
    {
        await Clients.Group($"group:{groupId}")
            .SendAsync("GroupRead", groupId, Context.User?.FindFirstValue(ClaimTypes.NameIdentifier));
    }
    
    public async Task TypingInConversation(string conversationId, string otherUserId)
        => await Clients.User(otherUserId).SendAsync("UserTyping", conversationId, Context.User?.FindFirstValue(ClaimTypes.NameIdentifier));

    public async Task StoppedTypingInConversation(string conversationId, string otherUserId)
        => await Clients.User(otherUserId).SendAsync("UserStoppedTyping", conversationId, Context.User?.FindFirstValue(ClaimTypes.NameIdentifier));

    public async Task TypingInGroup(string groupId)
        => await Clients.OthersInGroup($"group : {groupId}").SendAsync("UserTypingInGroup", groupId, Context.User?.FindFirstValue(ClaimTypes.NameIdentifier));

    public async Task StoppedTypingInGroup(string groupId)
        => await Clients.OthersInGroup($"group : {groupId}").SendAsync("UserStoppedTypingInGroup", groupId,
            Context.User?.FindFirstValue(ClaimTypes.NameIdentifier));
    
    public async Task DeleteDirectMessage(string conversationId, string messageId, string otherUserId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        await Clients.User(otherUserId).SendAsync("DirectMessageDeleted", conversationId, messageId);
        await Clients.User(userId).SendAsync("DirectMessageDeleted", conversationId, messageId);
    }
    public async Task DeleteGroupMessage(string groupId, string messageId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        await Clients.Group($"group : {groupId}")
            .SendAsync("GroupMessageDeleted", groupId, messageId);
    }

}