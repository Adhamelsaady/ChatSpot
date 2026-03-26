using ChatSpot.Contracts.Persistence;
using ChatSpot.Dtos.Responses;
using ChatSpot.Models.NoSQL;
using ChatSpot.ResourceParameters;
using MongoDB.Driver;

namespace ChatSpot.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly MongoDbContext _db;

    public ConversationRepository(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<ConversationDocument?> GetByIdAsync(string conversationId)
    {
        Console.WriteLine(conversationId);
        return await _db.Conversations.Find(c => c.Id == conversationId).FirstOrDefaultAsync();
    }
    public async Task<PagedResult<ConversationDocument>> GetAllConversations(BaseResourceParameter resourceParameter,
        string userId)
    {
        var conversation =  await _db.Conversations
            .Find(Builders<ConversationDocument>.Filter.AnyEq(c => c.Participants, userId))
            .SortByDescending(c => c.LastUpdated)
            .Skip((resourceParameter.PageNumber - 1) * resourceParameter.PageSize).Limit(resourceParameter.PageSize)
            .ToListAsync();
        return new PagedResult<ConversationDocument>()
        {
            Items = conversation,
            PageNumber = resourceParameter.PageNumber,
            PageSize = resourceParameter.PageSize
        };
    }

    public async Task<ConversationDocument?> GetByParticipantsAsync(string userId1, string userId2)
    {
        var filter = Builders<ConversationDocument>.Filter.And(
            Builders<ConversationDocument>.Filter.AnyEq(c => c.Participants, userId1),
            Builders<ConversationDocument>.Filter.AnyEq(c => c.Participants, userId2));
        return await _db.Conversations.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<ConversationDocument> UpdateLastMessage(string conversationId, string messageId)
    {
        var conversation = await GetByIdAsync(conversationId);
        var update = Builders<ConversationDocument>.Update.Set(c => c.LastMessageId, messageId);
        await _db.Conversations.UpdateOneAsync(c => c.Id == conversationId, update);
        return conversation;
    }
    public async Task<ConversationDocument> UpsertAsync(string conversationId , string senderId, string recipientId, string messageContent)
    {
        var conversation = await GetByIdAsync(conversationId);
        if (conversation != null)
        {
            
            var update = Builders<ConversationDocument>.Update
                .Set(c => c.LastMessage, messageContent)
                .Set(c => c.LastUpdated, DateTime.UtcNow)
                .Set(c => c.LastMessage , messageContent)
                .Inc($"unreadCount.{recipientId}", 1);
            
            await _db.Conversations.UpdateOneAsync(c => c.Id == conversation.Id, update);
            return conversation;
        }

        var conversationToReturn = new ConversationDocument()
        {
            Participants = new List<string> { senderId, recipientId },
            LastMessage = messageContent,
            LastUpdated = DateTime.UtcNow,
            UnreadCount = new Dictionary<string, int> { { recipientId, 1 } }
        };
        await _db.Conversations.InsertOneAsync(conversationToReturn);
        return conversationToReturn;
    }

    public async Task<ConversationDocument?> CreateConversation(string user1Id, string user2Id)
    {
       await _db.Conversations.InsertOneAsync(new ConversationDocument() {Participants = {user1Id, user2Id}});
       return await GetByParticipantsAsync(user1Id, user2Id);
    }

    public async Task MarkConversationAsRead(string conversationId , string userId)
    {
        var conversation = await GetByIdAsync(conversationId);
        var lastMessageId = conversation.LastMessageId;
        var update = Builders<ConversationDocument>.Update
            .Set(c => c.UnreadCount[userId], 0)
            .Set(c => c.LastReadMessageId, lastMessageId);
        await  _db.Conversations.UpdateOneAsync(c => c.Id == conversation.Id, update);
    }

    public async Task UpdateLastMessageSnapshotAsync(string conversationId, string lastMessageId, string messageContent,
        DateTime lastUpdated)
    {
        var update = Builders<ConversationDocument>.Update
            .Set(c => c.LastMessageId, lastMessageId)
            .Set(c => c.LastMessage, messageContent)
            .Set(c => c.LastUpdated, lastUpdated);
        await _db.Conversations.UpdateOneAsync(c => c.Id == conversationId, update);
    }

    public async Task ClearLastMessageSnapshotAsync(string conversationId)
    {
        var update = Builders<ConversationDocument>.Update
            .Set(c => c.LastMessageId, string.Empty)
            .Set(c => c.LastMessage, string.Empty)
            .Set(c => c.LastUpdated, DateTime.UtcNow);
        await _db.Conversations.UpdateOneAsync(c => c.Id == conversationId, update);
    }
}