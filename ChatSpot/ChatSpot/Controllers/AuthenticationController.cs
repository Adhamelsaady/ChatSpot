using ChatSpot.Contracts.Services;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Models.NoSQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChatSpot.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public AuthenticationController(IAuthenticationService authenticationService , TokenValidationParameters tokenValidationParameters)
    {
        _authenticationService = authenticationService;
        _tokenValidationParameters = tokenValidationParameters;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authenticationService.Register(registerDto);
        return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(RegisterationConfirmationDto registerationConfirmationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var result = await _authenticationService.ConfirmEmail(registerationConfirmationDto);
        return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var result = await _authenticationService.Login(loginDto);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenDto refreshTokenDto)
    {
        var jwtOptions = HttpContext.RequestServices
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);
        var validationParameters = jwtOptions.TokenValidationParameters.Clone();
        validationParameters.ValidateLifetime = false;
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var result = await _authenticationService.RefreshToken(refreshTokenDto);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutDto logoutDto)
    {
        await _authenticationService.Logout(logoutDto);
        return Ok();
    }
    
}