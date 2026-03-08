using ChatSpot.Contracts.Persistence;
using ChatSpot.Models.NoSQL;
using MongoDB.Driver;

namespace ChatSpot.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly MongoDbContext _db;

    public ConversationRepository(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<ConversationDocument?> GetByParticipantsAsync(string userId1, string userId2)
    {
        var filter = Builders<ConversationDocument>.Filter.And(
            Builders<ConversationDocument>.Filter.AnyEq(c => c.Participants, userId1),
            Builders<ConversationDocument>.Filter.AnyEq(c => c.Participants, userId2));
        return await _db.Conversations.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<ConversationDocument> UpsertAsync(string senderId, string recipientId, string messageContent)
    {
        var existing = await GetByParticipantsAsync(senderId, recipientId);
        if (existing != null)
        {
            var update = Builders<ConversationDocument>.Update
                .Set(c => c.LastMessage, messageContent)
                .Set(c => c.LastUpdated, DateTime.UtcNow)
                .Inc($"unreadCount.{recipientId}", 1);
            await _db.Conversations.UpdateOneAsync(c => c.Id == existing.Id, update);
            existing.LastMessage = messageContent;
            return existing;
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
}