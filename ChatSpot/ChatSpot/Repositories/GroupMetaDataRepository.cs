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
    
    public async Task<GroupChatMetaDocument> GetOrCreateAsync(Guid groupId)
    {
        var id = groupId.ToString();
        var meta = await GetByGroupIdAsync(id);
        if (meta != null) return meta;
        meta = new GroupChatMetaDocument { GroupId = id };
        await _db.GroupChatMeta.InsertOneAsync(meta);
        return meta;
    }
    public async Task<IList<GroupChatMetaDocument>> GetByGroupIdsAsync(IEnumerable<string> groupIds)
    {
        if (groupIds == null || !groupIds.Any())
            return new List<GroupChatMetaDocument>();
        
        return await _db.GroupChatMeta
            .Find(m => groupIds.Contains(m.GroupId)).ToListAsync();
    }
}