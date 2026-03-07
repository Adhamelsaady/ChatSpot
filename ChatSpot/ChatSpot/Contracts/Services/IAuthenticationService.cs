using ChatSpot.Dtos;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;

namespace ChatSpot.Contracts.Services;

public interface IAuthenticationService
{
    Task<BaseResponse> Register(RegisterDto registerDto);
    Task<BaseResponse> ConfirmEmail (RegisterationConfirmationDto registerationConfirmationDto);
    Task<AuthResult> Login(LoginDto loginDto);
    
    Task <AuthResult> RefreshToken(RefreshTokenDto refreshTokenDto);
}