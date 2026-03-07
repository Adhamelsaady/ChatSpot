using ChatSpot.Models.SQL;

namespace ChatSpot.Contracts.Persistence;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken);
    Task<bool> MarkRefreshTokenAsUsedAsync(RefreshToken refreshToken);
}