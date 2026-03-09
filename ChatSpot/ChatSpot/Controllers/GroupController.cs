using System.Security.Claims;
using ChatSpot.Contracts.Persistence;
using ChatSpot.Contracts.Services;
using ChatSpot.Dtos.Ingoing;
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

    [HttpPost("create")]
    public async Task<IActionResult> CreateGroup([FromBody] GroupToCreateDto createGroupDt)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await _groupService.CreateGroup(createGroupDt, currentUserId);
        return Ok(result);
    }
}