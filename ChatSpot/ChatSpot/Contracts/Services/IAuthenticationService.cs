using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;

namespace ChatSpot.Contracts.Services;

public interface IAuthenticationService
{
    AuthResult Register(RegisterDto registerDto);
}