using ChatSpot.Contracts.Persistence;
using ChatSpot.Dtos.Responses;
using ChatSpot.Models.NoSQL;
using ChatSpot.ResourceParameters;
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
    
    public async Task<PagedResult<MessageDocument>> GetMessagesOfConversationAsync(BaseResourceParameter resourceParameter , string conversationId)
    {
        var messages =
            await _db.Messages.Find(Builders<MessageDocument>.Filter.Eq(m => m.ConversationId , conversationId))
                .SortByDescending(m => m.Timestamp)
                .Skip((resourceParameter.PageNumber - 1) * resourceParameter.PageSize)
                .Limit(resourceParameter.PageSize)
                .ToListAsync();
        return new PagedResult<MessageDocument>()
        {
            Items = messages,
            PageNumber = resourceParameter.PageNumber,
            PageSize = resourceParameter.PageSize
        };
    }
}