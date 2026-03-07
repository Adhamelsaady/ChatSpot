using AutoMapper;
using ChatSpot.Contracts.Infrastructure;
using ChatSpot.Contracts.Services;
using ChatSpot.Dtos;
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

    public async Task<BaseResponse> Register(RegisterDto registerDto)
    {
        var user = await _userManager.FindByEmailAsync(registerDto.Email);
        if (user != null)
        {
            if (user.EmailConfirmed ==  false)
            {
                await _userManager.DeleteAsync(user);
            }
            else
            {
                return new BaseResponse()
                {
                    IsSuccess = false,
                    Message = $"User with email : {registerDto.Email}  already exists"
                };
            }
        }
        
        var otp = _otpService.GenerateOtp();
        var userToAdd = _mapper.Map<ApplicationUser>(registerDto);
        userToAdd.Otp = otp;
        userToAdd.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
        var result = await _userManager.CreateAsync(userToAdd , registerDto.Password);
        if (result.Succeeded == false)
        {
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = result.Errors.First().Description
            };
        }
        await _emailService.SendEmailConfirmationOtpAsync(registerDto.Email , registerDto.UserName , otp);
        return new BaseResponse()
        {
            IsSuccess = true,
            Message = $"Check your email : {registerDto.Email}"
        };
    }
}