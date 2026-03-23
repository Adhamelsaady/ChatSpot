using ChatSpot.Contracts.Persistence;
using ChatSpot.Models.NoSQL;
using MongoDB.Driver;

namespace ChatSpot.Repositories;

public class GroupMetaDataRepository : IGroupMetaDataRepository
{
    private readonly MongoDbContext _db;


    public  GroupMetaDataRepository(MongoDbContext db)
    {
        _db = db;
    }
    public async Task<GroupChatMetaDocument?> GetByGroupIdAsync(string groupId) =>
        await _db.GroupChatMeta.Find(g => g.GroupId == groupId).FirstOrDefaultAsync();

    public Task<GroupChatMetaDocument> GetOrCreateAsync(Guid groupId)
    {
        throw new NotImplementedException();
    }


    public async Task<GroupChatMetaDocument> GetOrCreateAsync(string groupId)
    {
       
        var filter = Builders<GroupChatMetaDocument>.Filter.Eq(x => x.GroupId, groupId);
        var update = Builders<GroupChatMetaDocument>.Update
            .SetOnInsert(x => x.GroupId, groupId);

        var options = new FindOneAndUpdateOptions<GroupChatMetaDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        return await _db.GroupChatMeta.FindOneAndUpdateAsync(filter, update, options);
    }
    public async Task<IList<GroupChatMetaDocument>> GetByGroupIdsAsync(IEnumerable<string> groupIds)
    {
        if (groupIds == null || !groupIds.Any())
            return new List<GroupChatMetaDocument>();
        
        return await _db.GroupChatMeta
            .Find(m => groupIds.Contains(m.GroupId)).ToListAsync();
    }
    
    public async Task MarkGroupMessagesAsRead(string groupId , string userId)
    {
        var meta = await GetByGroupIdAsync(groupId);
        if (meta == null)
        {
            meta = await GetOrCreateAsync(groupId);
        }
        var lastMessageId = meta.LastMessageId;
        var update = Builders<GroupChatMetaDocument>.Update
            .Set(c => c.UnreadCount[userId], 0)
            .Set(c => c.LastReadMessageId, lastMessageId);
        await  _db.GroupChatMeta.UpdateOneAsync(c => c.Id == meta.Id, update);
    }

    public async Task<GroupChatMetaDocument> UpsertAsync(string groupId , string messageContent , string userId)
    {
        var meta = await GetOrCreateAsync(groupId);
            var update = Builders<GroupChatMetaDocument>.Update
                .Set(c => c.LastMessage, messageContent)
                .Set(c => c.LastUpdated, DateTime.UtcNow)
                .Set(c => c.LastMessage, messageContent);
            await _db.GroupChatMeta.UpdateOneAsync(c => c.Id == meta.Id, update);
            return meta;
        return meta;
    }
    
    public async Task IncrementUnreadAsync(string groupId, List<string> exceptUserIds)
    {
        var meta = await GetByGroupIdAsync(groupId);
        if (meta == null) return;

        var filter = Builders<GroupChatMetaDocument>.Filter.Eq(g => g.GroupId, groupId);
        var updates = exceptUserIds.Select(uid =>
            Builders<GroupChatMetaDocument>.Update.Inc($"unreadCount.{uid}", 1)).ToList();

        if (updates.Any())
            await _db.GroupChatMeta.UpdateOneAsync(filter,
                Builders<GroupChatMetaDocument>.Update.Combine(updates));
    }

    public async Task<GroupChatMetaDocument> UpdateLastMessage(string groupId, string messageId)
    {
        var meta = await GetByGroupIdAsync (groupId);
        var update = Builders<GroupChatMetaDocument>.Update.Set(c => c.LastMessageId, messageId);
        await _db.GroupChatMeta.UpdateOneAsync(c => c.GroupId == groupId, update);
        return meta;
    }
}