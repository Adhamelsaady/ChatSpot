namespace ChatSpot.Models.SQL;

public class UserConnection
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
    public DateTime? DisconnectedAt { get; set; }
    public bool IsConnected { get; set; }
}