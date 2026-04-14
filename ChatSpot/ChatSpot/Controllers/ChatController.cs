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
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationDto createConversationDto)
    {
        var result = await _chatService.CreateConversationAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value! , createConversationDto.OtherUserId);
        return Ok(new {conversationId = result});
    }
    [HttpGet]
    public async Task<IActionResult> GetConversations([FromQuery] BaseResourceParameter baseResourceParameter)
    {
        var result = await _chatService.GetAllConversationsAsync(baseResourceParameter , User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        return Ok(result);
    }
    
    [HttpGet("{conversationId}")]
    public async Task<IActionResult> GetMessagesOfConversation([FromQuery] BaseResourceParameter baseResourceParameter, [FromRoute]string conversationId)
    {
        var result = await _chatService.GetMessagesOfConversationAsync(baseResourceParameter, conversationId , User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        return Ok(result);
    }
    
    [HttpPost("{conversationId}")]
    public async Task<IActionResult> SendMessage([FromForm] MessageForSending messageForSending , [FromRoute] string conversationId)
    {
        if (!ModelState.IsValid)
        {
            return  BadRequest();
        }
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var cuurentUserName = User.FindFirst(ClaimTypes.Name)?.Value;
        var result= await _chatService.SendMessageAsync(messageForSending , currentUserId , cuurentUserName , conversationId);
        if(result.IsSuccess) return Ok(result);
        else return  BadRequest(result);
    }

    [HttpDelete("{conversationId}")]
    public async Task<IActionResult> DeleteMessage([FromRoute] string conversationId,
        [FromBody] DeleteMessageDto deleteMessageDto)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await _chatService.DeleteMessageAsync(deleteMessageDto.MessageId, currentUserId);
        if(result.IsSuccess) return NoContent();
        else return BadRequest(result);
    }
    
}