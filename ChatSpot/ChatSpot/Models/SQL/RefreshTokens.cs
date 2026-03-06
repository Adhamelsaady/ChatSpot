namespace ChatSpot.Models.SQL;

public class RefreshTokens
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Token { get; set; }
    public string JwtId { get; set; }
    public string UserId { get; set; }
    public bool isUsed { get; set; }
    public bool isRevoked { get; set; }
    
    public ApplicationUser User { get; set; }
}