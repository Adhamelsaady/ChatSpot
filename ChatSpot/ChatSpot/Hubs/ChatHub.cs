using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatSpot.Hubs;

[Authorize]
public class ChatHub : Hub
{
   
}