using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using ChatSpot.Contracts.Infrastructure;
using ChatSpot.Contracts.Persistence;
using ChatSpot.Contracts.Services;
using ChatSpot.Dtos;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;
using ChatSpot.Models.NoSQL;
using ChatSpot.Models.SQL;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace ChatSpot.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IOtpService _otpService;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenGeneration _jwtTokenGeneration;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly MongoDbContext _mongoDbContext;
    
    public AuthenticationService(
        MongoDbContext mongoDbContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOtpService otpService,
        IMapper mapper,
        IEmailService emailService,
        IJwtTokenGeneration jwtTokenGeneration,
        TokenValidationParameters tokenValidationParameters ,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _otpService = otpService;
        _mapper = mapper;
        _emailService = emailService;
        _jwtTokenGeneration = jwtTokenGeneration;
        _tokenValidationParameters = tokenValidationParameters;
        _refreshTokenRepository = refreshTokenRepository;
        _mongoDbContext = mongoDbContext;
    }

    public async Task<BaseResponse> Register(RegisterDto registerDto)
    {
        var user = await _userManager.FindByEmailAsync(registerDto.Email);
        if (user != null)
        {
            if (user.EmailConfirmed == false)
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

        var userToAdd = _mapper.Map<ApplicationUser>(registerDto);
        userToAdd.Otp = _otpService.GenerateOtp();
        userToAdd.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
        var result = await _userManager.CreateAsync(userToAdd, registerDto.Password);
        if (result.Succeeded == false)
        {
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = result.Errors.First().Description
            };
        }

        await _emailService.SendEmailConfirmationOtpAsync(registerDto.Email, registerDto.UserName, userToAdd.Otp);
        return new BaseResponse()
        {
            IsSuccess = true,
            Message = $"Check your email : {registerDto.Email}"
        };
    }

    public async Task<BaseResponse> ConfirmEmail(RegisterationConfirmationDto registerationConfirmationDto)
    {
        var user = await _userManager.FindByEmailAsync(registerationConfirmationDto.Email);
        if (user == null || user.EmailConfirmed)
        {
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "User not found"
            };
        }

        if (user.Otp != registerationConfirmationDto.Otp)
        {
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "Invalid OTP"
            };
        }

        if (user.OtpExpiry > DateTime.UtcNow)
        {
            return new BaseResponse()
            {
                IsSuccess = false,
                Message = "Expired OTP"
            };
        }

        user.EmailConfirmed = true;
        user.Otp = null;
        user.OtpExpiry = null;
        await _userManager.UpdateAsync(user);
        return new BaseResponse()
        {
            IsSuccess = true,
            Message = $"User with email : {registerationConfirmationDto.Email} has been registered"
        };
    }

    public async Task<AuthResult> Login(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !user.EmailConfirmed)
        {
            return new AuthResult()
            {
                IsSuccess = false,
                Message = "Wrong email or password"
            };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
        {
            return new AuthResult()
            {
                IsSuccess = false,
                Message = "Wrong email or password"
            };
        }

        var authResult = await _jwtTokenGeneration.GenerateJwtToken(user);
        authResult.IsSuccess = true;
        authResult.Message = "Login successful";
        return authResult;
    }

    public async Task<AuthResult> RefreshToken(RefreshTokenDto refreshTokenDto)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal =
            tokenHandler.ValidateToken(refreshTokenDto.Token, _tokenValidationParameters, out var validatedToken);
        if (validatedToken is JwtSecurityToken jwtSecurityToken)
        {
            var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.CurrentCultureIgnoreCase);
            if (!result)
            {
                return null;
            }
        }

        var utcExpiryDate =
            long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)!.Value);
        var expiryDate = UnixTimeToDateTime(utcExpiryDate);
        if (expiryDate > DateTime.UtcNow)
        {
            return new AuthResult()
            {
                IsSuccess = false, Message = "The Token Is Not Expired"
            };
        }

        var refreshTokenEntity = await _refreshTokenRepository.GetRefreshTokenAsync(refreshTokenDto.RefreshToken);

        var jti = principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
        if (refreshTokenEntity == null || refreshTokenEntity.isUsed == true || refreshTokenEntity.isRevoked == true
            || refreshTokenEntity.ExpiresAt < DateTime.UtcNow || jti != refreshTokenEntity.JwtId)
        {
            return new AuthResult()
            {
                IsSuccess = false, Message = "Invalid Refresh Tokennnn"
            };
        }

        if (!await _refreshTokenRepository.MarkRefreshTokenAsUsedAsync(refreshTokenEntity))
        {
            return new AuthResult()
            {
                IsSuccess = false, Message = "Something went wrong"
            };
        }

        var tokenResult = await _jwtTokenGeneration.GenerateJwtToken(refreshTokenEntity.User);
        return new AuthResult()
        {
            IsSuccess = true,
            RefreshToken = tokenResult.RefreshToken,
            Token = tokenResult.Token,
        };
    }

    private DateTime UnixTimeToDateTime(long unixTime)
    {
        var result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return result.AddSeconds(unixTime).ToUniversalTime();
    }
}