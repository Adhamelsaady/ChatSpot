using ChatSpot.Contracts.Persistence;
using ChatSpot.Models.SQL;
using Microsoft.EntityFrameworkCore;

namespace ChatSpot.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ChatSpotDbContext _db;

    public RefreshTokenRepository(ChatSpotDbContext db)
    {
        _db = db;
    }
    public async Task<RefreshToken> GetRefreshTokenAsync(string refreshToken)
    {
        return await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
    }

    public async Task<bool> MarkRefreshTokenAsUsedAsync(RefreshToken refreshToken)
    {
        refreshToken.isUsed =  true;
        return await _db.SaveChangesAsync() > 0;
    }
}