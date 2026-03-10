using System.Security.Claims;
using ChatSpot.Contracts.Services;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.ResourceParameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatSpot.Controllers;


[ApiController]
[Route("/api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("create-conversation")]
    public async Task<IActionResult> CreateConversation([FromBody] string otherId)
    {
        var result = await _chatService.CreateConversation(User.FindFirst(ClaimTypes.NameIdentifier)?.Value! , otherId);
        return Ok(new {conversationId = result});
    }
    [HttpGet]
    public async Task<IActionResult> GetConversations([FromQuery] BaseResourceParameter baseResourceParameter)
    {
        var result = await _chatService.GetAllConversations(baseResourceParameter , User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        return Ok(result);
    }
    
    [HttpGet("{conversationId}")]
    public async Task<IActionResult> GetMessagesOfConversation([FromQuery] BaseResourceParameter baseResourceParameter, [FromRoute]string conversationId)
    {
        var result = await _chatService.GetMessagesOfConversation(baseResourceParameter, conversationId);
        return Ok(result);
    }
    
    [HttpPost("{conversationId}/send-message")]
    public async Task<IActionResult> SendMessage([FromBody] MessageForSending messageForSending , [FromRoute] string conversationId)
    {
        if (!ModelState.IsValid || string.IsNullOrEmpty(messageForSending.ReceiverId))
        {
            return  BadRequest();
        }
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result =await _chatService.SendMessage(messageForSending , currentUserId , conversationId);
        if(result.IsSuccess) return Ok(result);
        else return  BadRequest(result);
    }
    
    
    
}