using AutoMapper;
using ChatSpot.Contracts.Infrastructure;
using ChatSpot.Contracts.Services;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;
using ChatSpot.Models.SQL;
using Microsoft.AspNetCore.Identity;

namespace ChatSpot.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOtpService _otpService;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenGeneration _jwtTokenGeneration;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        IOtpService otpService,
        IMapper mapper,
        IEmailService emailService,
        IJwtTokenGeneration jwtTokenGeneration)
    {
        _userManager = userManager;
        _otpService = otpService;
        _mapper = mapper;
        _emailService = emailService;
        _jwtTokenGeneration = jwtTokenGeneration;
    }

    public AuthResult Register(RegisterDto registerDto)
    {
        throw new NotImplementedException();
    }
}