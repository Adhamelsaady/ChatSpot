using ChatSpot.Dtos.Outgoing;
using ChatSpot.Models.SQL;

namespace ChatSpot.Contracts.Infrastructure;

public interface IJwtTokenGeneration
{
    Task<AuthResult> GenerateJwtToken(ApplicationUser user);
    string GenerateRefreshToken();
}