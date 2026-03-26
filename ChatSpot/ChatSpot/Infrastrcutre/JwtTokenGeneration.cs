using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChatSpot.Contracts.Infrastructure;
using ChatSpot.Contracts.Persistence;
using ChatSpot.Dtos.Outgoing;
using ChatSpot.Models.SQL;
using Microsoft.IdentityModel.Tokens;

namespace ChatSpot.Infrastrcutre;

public class JwtTokenGeneration : IJwtTokenGeneration
{
    private readonly IConfiguration _configuration;
    private readonly IBaseRepository<RefreshToken> _refreshTokensRepository;
    public JwtTokenGeneration(IConfiguration configuration, IBaseRepository<RefreshToken> refreshTokensRepository)
    {
        _configuration = configuration;
        _refreshTokensRepository = refreshTokensRepository;
    }
    public async Task<AuthResult> GenerateJwtToken(ApplicationUser user)
    {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier ,  user.Id),
            new  Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, user.Email),
        };
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(_configuration.GetValue<int>("Jwt:Lifetime")),
            signingCredentials: creds);
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = new RefreshToken()
        {
            CreatedAt = DateTime.UtcNow,
            Token = $"{GenerateRefreshToken()}_{Guid.NewGuid()}",
            UserId = user.Id,
            isRevoked = false,
            isUsed = false,
            JwtId = token.Id,
            ExpiresAt = DateTime.UtcNow.AddMonths(1)
        };
        await _refreshTokensRepository.AddAsync(refreshToken);
        var result = new AuthResult()
        {
            Token = tokenString,
            RefreshToken = refreshToken.Token,
        };
        return result;
    }

    public string GenerateRefreshToken()
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@!$#_";
        return new string(Enumerable.Repeat(chars, 25).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}