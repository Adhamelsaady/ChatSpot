using System.Security.Claims;
using ChatSpot.Contracts.Services;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.ResourceParameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatSpot.Controllers;


[ApiController]
[Route("[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet]
    public async Task<IActionResult> GetConversation([FromQuery] BaseResourceParameter baseResourceParameter)
    {
        var result = await _chatService.GetAllConversations(baseResourceParameter , User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        return Ok(result);
    }
    
    [HttpPost("send-message")]
    public async Task<IActionResult> SendMessage([FromBody] MessageForSending messageForSending)
    {
        if (!ModelState.IsValid || string.IsNullOrEmpty(messageForSending.ReceiverId))
        {
            return  BadRequest();
        }
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result =await _chatService.SendMessage(messageForSending , currentUserId);
        if(result.IsSuccess) return Ok(result);
        else return  BadRequest(result);
    }
}