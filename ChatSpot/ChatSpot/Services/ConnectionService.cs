using ChatSpot.Contracts.Persistence;
using ChatSpot.Contracts.Services;
using ChatSpot.Models.SQL;
using Microsoft.EntityFrameworkCore;

namespace ChatSpot.Services;

public class ConnectionService : IConnectionService
{
    private readonly ChatSpotDbContext _db;
    private readonly IBaseRepository<ApplicationUser> _userRepository;

    public ConnectionService(ChatSpotDbContext db, IBaseRepository<ApplicationUser> userRepository)
    {
        _db = db;
        _userRepository = userRepository;
    }

    public async Task ConnectAsync(string userId, string connectionId)
    {
        var existingConnection = await _db.UserConnections.FirstOrDefaultAsync(c => c.UserId == userId);
        if (existingConnection != null)
        {
            existingConnection.ConnectionId = connectionId;
            existingConnection.IsConnected = true;
            existingConnection.ConnectedAt = DateTime.UtcNow;
            existingConnection.DisconnectedAt = null;
            _db.UserConnections.Update(existingConnection);
        }
        else
        {
            _db.UserConnections.Add(new UserConnection
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ConnectionId = connectionId,
                ConnectedAt = DateTime.UtcNow,
                IsConnected = true
            });
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            user.isOnline = true;
         
        }

        await _db.SaveChangesAsync();
    }

    public async Task DisconnectAsync(string connectionId)
    {
        var connection = await _db.UserConnections
            .FirstOrDefaultAsync(c => c.ConnectionId == connectionId && c.IsConnected);
        if (connection == null) return;

        connection.IsConnected = false;
        connection.DisconnectedAt = DateTime.UtcNow;
        _db.UserConnections.Update(connection);
        var user = await _userRepository.GetByIdAsync(connection.UserId);
        if (user != null)
        {
            user.isOnline = false;
            user.LastSeen = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<List<UserConnection>> GetOnlineUsersAsync()
    {
        return await _db.UserConnections.Where(c => c.IsConnected).ToListAsync();
    }

    public async Task<List<UserConnection>> GetUserConnectionsAsync(string userId)
    {
        return await _db.UserConnections.Where(c => c.UserId == userId && c.IsConnected).ToListAsync();
    }
}