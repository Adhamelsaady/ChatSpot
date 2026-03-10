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
    public async Task<IActionResult> GetById([FromRoute] Guid groupId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _groupService.GetGroup(groupId, currentUserId);
        return result.IsSuccess ? Ok(result) :  BadRequest("problem");
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
        return result.IsSuccess ? Ok(result) :  BadRequest(result.Message);
    }
}