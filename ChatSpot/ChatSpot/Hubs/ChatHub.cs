using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatSpot.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private static readonly Dictionary<string, string> _connections = new();
    public static async Task SendToUserAsync(IHubContext<ChatHub> hub, string userId, string method, object payload)
    {
        if (_connections.TryGetValue(userId, out var connId))
            await hub.Clients.Client(connId).SendAsync(method, payload);
    }
}