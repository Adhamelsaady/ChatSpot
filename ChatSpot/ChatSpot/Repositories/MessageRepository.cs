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
        var filter =  Builders<MessageDocument>.Filter.Eq(m => m.ConversationId, conversationId) 
            & Builders<MessageDocument>.Filter.Eq(m => m.IsDeleted, false);
        var messages =
            await _db.Messages.Find(filter)
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

    public Task<PagedResult<MessageDocument>> GetMessagesOfDocument(BaseResourceParameter resourceParameter, string groupId)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResult<MessageDocument>> GetMessagesOfGroup(BaseResourceParameter resourceParameter , string groupId)
    {
        var messages =
            await _db.Messages.Find(Builders<MessageDocument>.Filter.Eq(m => m.GroupId , groupId))
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

    public async Task<bool> DeleteMessageAsync(string messageId)
    {
        var filter = Builders<MessageDocument>.Filter.Eq(m => m.Id, messageId);
        var update = Builders<MessageDocument>.Update
            .Set(m => m.IsDeleted, true);
        var result = await _db.Messages.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }
}