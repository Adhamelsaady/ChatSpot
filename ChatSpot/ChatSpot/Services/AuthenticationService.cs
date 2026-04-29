using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using ChatSpot.Contracts.Infrastructure;
using ChatSpot.Contracts.Persistence;
using ChatSpot.Contracts.Services;
using ChatSpot.Dtos;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;
using ChatSpot.Models.SQL;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

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
    private readonly Cloudinary _cloudinary;
    private readonly IConfiguration _configuration;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOtpService otpService,
        IMapper mapper,
        IEmailService emailService,
        IJwtTokenGeneration jwtTokenGeneration,
        TokenValidationParameters tokenValidationParameters,
        IRefreshTokenRepository refreshTokenRepository, 
        Cloudinary cloudinary,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _otpService = otpService;
        _mapper = mapper;
        _emailService = emailService;
        _jwtTokenGeneration = jwtTokenGeneration;
        _tokenValidationParameters = tokenValidationParameters;
        _refreshTokenRepository = refreshTokenRepository;
        _cloudinary = cloudinary;
        _configuration = configuration;
    }

    public async Task<AuthResult> GoogleLogin(GoogleLoginDto googleLoginDto)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings();
            var clientId = _configuration["GoogleAuth:ClientId"];
            if (!string.IsNullOrEmpty(clientId))
            {
                settings.Audience = new[] { clientId };
            }

            var payload = await GoogleJsonWebSignature.ValidateAsync(googleLoginDto.IdToken, settings);
            
            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = payload.Email.Split('@')[0] + "_" + Guid.NewGuid().ToString("N").Substring(0, 4),
                    Email = payload.Email,
                    EmailConfirmed = true,
                    FirstName = payload.GivenName,
                    LastName = payload.FamilyName,
                    ProfilePicture = payload.Picture
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    return new AuthResult { IsSuccess = false, Message = "Failed to create user account." };
                }
            }
            
            var authResult = await _jwtTokenGeneration.GenerateJwtToken(user);
            authResult.IsSuccess = true;
            authResult.Message = "Login successful";
            authResult.ProfilePicture = user.ProfilePicture ?? string.Empty;
            return authResult;
        }
        catch (InvalidJwtException)
        {
            return new AuthResult { IsSuccess = false, Message = "Invalid Google ID Token." };
        }
        catch (Exception ex)
        {
            return new AuthResult { IsSuccess = false, Message = "An error occurred during Google Login." };
        }
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

        if (registerDto.ProfilePicture != null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(registerDto.ProfilePicture.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
                return new BaseResponse { IsSuccess = false, Message = "Only image files are allowed." };
            if (registerDto.ProfilePicture.Length > 5 * 1024 * 1024)
                return new BaseResponse { IsSuccess = false, Message = "Profile picture must be under 5MB." };
            using var stream = registerDto.ProfilePicture.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(registerDto.ProfilePicture.FileName, stream),
                Folder = "chatspot/profile-pictures",
                Transformation = new Transformation().Width(300).Height(300).Crop("fill").Gravity("face")
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
                return new BaseResponse { IsSuccess = false, Message = "Failed to upload profile picture." };

            userToAdd.ProfilePicture = uploadResult.SecureUrl.ToString();
        }
        
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

        if (user.OtpExpiry < DateTime.UtcNow)
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
        var user = await _userManager.FindByNameAsync(loginDto.EmailOrUserName)
                   ?? await _userManager.FindByEmailAsync(loginDto.EmailOrUserName);
        if (user == null || !user.EmailConfirmed)
        {
            return new AuthResult()
            {
                IsSuccess = false,
                Message = "Wrong email/username or password"
            };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
        {
            return new AuthResult()
            {
                IsSuccess = false,
                Message = "Wrong email/username or password"
            };
        }

        var authResult = await _jwtTokenGeneration.GenerateJwtToken(user);
        authResult.IsSuccess = true;
        authResult.Message = "Login successful";
        authResult.ProfilePicture = user.ProfilePicture ?? string.Empty;
        return authResult;
    }

    public async Task<AuthResult> RefreshToken(RefreshTokenDto refreshTokenDto)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        _tokenValidationParameters.ValidateLifetime = false;
        var principal =
            tokenHandler.ValidateToken(refreshTokenDto.Token, _tokenValidationParameters, out var validatedToken);
        if (validatedToken is JwtSecurityToken jwtSecurityToken)
        {
            var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.CurrentCultureIgnoreCase);
            if (!result)
            {
                return new AuthResult()
                {
                    IsSuccess = false, Message = "The Token Is Not Expired"
                };
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
                IsSuccess = false, Message = "Invalid Refresh Token"
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

    public async Task<bool> Logout(LogoutDto logoutDto)
    {
        var result = await _refreshTokenRepository.MarkRefreshTokenAsRevokedAsync(logoutDto.RefreshToken);
        return result;
    }

    public async Task<BaseResponse> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
        if (user == null || !user.EmailConfirmed)
        {
            return new BaseResponse { IsSuccess = true, Message = $"If that email is registered, a reset code has been sent." };
        }

        user.Otp = _otpService.GenerateOtp();
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
        await _userManager.UpdateAsync(user);

        var firstName = user.FirstName ?? user.UserName ?? "User";
        await _emailService.SendPasswordResetOtpAsync(user.Email!, firstName, user.Otp);

        return new BaseResponse { IsSuccess = true, Message = $"If that email is registered, a reset code has been sent." };
    }

    public async Task<BaseResponse> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
        if (user == null || !user.EmailConfirmed)
        {
            return new BaseResponse { IsSuccess = false, Message = "Invalid request." };
        }

        if (user.Otp != resetPasswordDto.Otp)
        {
            return new BaseResponse { IsSuccess = false, Message = "Invalid OTP." };
        }

        if (user.OtpExpiry < DateTime.UtcNow)
        {
            return new BaseResponse { IsSuccess = false, Message = "OTP has expired." };
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, resetPasswordDto.NewPassword);

        if (!result.Succeeded)
        {
            return new BaseResponse { IsSuccess = false, Message = result.Errors.First().Description };
        }

        user.Otp = null;
        user.OtpExpiry = null;
        await _userManager.UpdateAsync(user);

        return new BaseResponse { IsSuccess = true, Message = "Password has been reset successfully." };
    }

    private DateTime UnixTimeToDateTime(long unixTime)
    {
        var result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return result.AddSeconds(unixTime).ToUniversalTime();
    }
}