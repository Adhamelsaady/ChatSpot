using ChatSpot.Models.SQL;
using Microsoft.EntityFrameworkCore;

namespace ChatSpot.Services;

public class ConnectionService
{
    private readonly ChatSpotDbContext _db; // replace with your actual DbContext name
    
    public ConnectionService(ChatSpotDbContext db)
    {
        _db = db;
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
        await _db.SaveChangesAsync();
    }

    public async Task DisconnectAsync(string connectionId)
    {
        var connection = await _db.UserConnections
            .FirstOrDefaultAsync(c => c.ConnectionId == connectionId && c.IsConnected);
        if (connection != null)
        {
            connection.IsConnected = false;
            connection.DisconnectedAt = DateTime.UtcNow;
            _db.UserConnections.Update(connection);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<UserConnection>> GetOnlineUsers()
    {
        return await _db.UserConnections.Where(c => c.IsConnected).ToListAsync();
    }

    public async Task<List<UserConnection>> GetUserConnections(string userId)
    {
        return await _db.UserConnections.Where(c => c.UserId == userId && c.IsConnected).ToListAsync();
    }
}