using System.Security.Claims;
using ChatSpot.Contracts.Services;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.ResourceParameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatSpot.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] BaseResourceParameter resourceParameter)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await _userService.SearchUsers(resourceParameter, currentUserId);
        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var profile = await _userService.GetMyProfile(userId);
        if (profile == null) return NotFound();
        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await _userService.UpdateProfile(userId, dto);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await _userService.ChangePassword(userId, dto);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}