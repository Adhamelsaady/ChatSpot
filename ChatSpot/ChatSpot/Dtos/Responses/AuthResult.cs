namespace ChatSpot.Dtos.Outgoing;

public class AuthResult : BaseResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}