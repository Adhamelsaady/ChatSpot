using ChatSpot.Models.SQL;

namespace ChatSpot.Contracts.Services;

public interface IConnectionService
{
    Task ConnectAsync(string userId, string connectionId);
    Task DisconnectAsync(string connectionId);
    Task<List<UserConnection>> GetOnlineUsersAsync();
    Task<List<UserConnection>> GetUserConnectionsAsync(string userId);
}