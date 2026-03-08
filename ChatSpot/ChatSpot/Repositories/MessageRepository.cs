using ChatSpot.Contracts.Persistence;
using ChatSpot.Models.NoSQL;
using MongoDB.Driver;

namespace ChatSpot.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly MongoDbContext _db;
    public MessageRepository(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<MessageDocument?> GetMessageByIdAsync(string id)
    {
        return await _db.Messages.Find(m => m.Id == id).FirstOrDefaultAsync();
    }

    public async Task<MessageDocument> CreateMessageAsync(MessageDocument message)
    {
        await _db.Messages.InsertOneAsync(message);
        return message;
    }
}