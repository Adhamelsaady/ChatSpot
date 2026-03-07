using ChatSpot.Contracts.Services;
using ChatSpot.Dtos.Ingoing;
using Microsoft.AspNetCore.Mvc;

namespace ChatSpot.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    public AuthenticationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return  BadRequest(ModelState);
        }
        var result = await _authenticationService.Register(registerDto);
        return Ok(result);
    }
}