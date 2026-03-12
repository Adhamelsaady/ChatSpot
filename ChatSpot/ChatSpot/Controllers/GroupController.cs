using System.Security.Claims;
using ChatSpot.Contracts.Persistence;
using ChatSpot.Contracts.Services;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.ResourceParameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatSpot.Controllers;

[ApiController]
[Route("api/group")]
[Authorize]
public class GroupController : ControllerBase
{
    private readonly IGroupService _groupService;
    public  GroupController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyGroups([FromQuery] BaseResourceParameter baseResourceParameter)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _groupService.GetMyGroups(baseResourceParameter, currentUserId);
        return Ok(result);
    }

    [HttpGet("{groupId::guid}")]
    public async Task<IActionResult> GetGroup([FromQuery] BaseResourceParameter baseResourceParameter , [FromRoute] Guid groupId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _groupService.GetGroupMessages(baseResourceParameter, groupId.ToString(), currentUserId);
        return Ok(result);
    }

    [HttpPost("{groupId::guid}/send-message")]
    public async Task<IActionResult> SendMessage([FromBody] MessageForSending messageForSending , [FromRoute] Guid groupId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _groupService.SendMessage(messageForSending, currentUserId , groupId);
        return Ok(result);
    }
    
    [HttpPost("create-group")]
    public async Task<IActionResult> CreateGroup([FromBody] GroupToCreateDto createGroupDt)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await _groupService.CreateGroup(createGroupDt, currentUserId);
        return Ok(result);
    }
    

    [HttpPost("{groupId::guid}/members")]
    public async Task<IActionResult> AddMembersToGroup([FromRoute] Guid groupId, GroupMemberToAddDto membersToAddDto)
    {
        var result = await _groupService.AddMembersToGroup(groupId, membersToAddDto ,  User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        return result.IsSuccess ? Ok(result) :  Forbid();
    }
    
    [HttpDelete("{groupId::guid}/delete-message")]
    public async Task<IActionResult> DeleteMessage([FromRoute] string groupId,
        [FromBody] DeleteMessageDto deleteMessageDto)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await _groupService.DeleteMessageAsync(deleteMessageDto.MessageId, currentUserId);
        if(result.IsSuccess) return NoContent();
        else return Forbid();
    }

    [HttpDelete("{groupId::guid}/remove/{targetUserId}")]
    public async Task<IActionResult> RemoveMember([FromRoute] Guid groupId, [FromRoute] string targetUserId)
    { 
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == targetUserId) return BadRequest();
        var result = await _groupService.RemoveMemberAsync(groupId, currentUserId, targetUserId);
        if (!result.IsSuccess)
        {
            return Forbid();
        }
        return NoContent();
    }

    [HttpDelete("{groupId::guid}/members/leave")]
    public async Task<IActionResult> LeaveGroup([FromRoute] Guid groupId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _groupService.LeaveGroupAsync(groupId, userId);
        if (!result.IsSuccess)
        {
            return Forbid();
        }
        return NoContent(); 
    }
}