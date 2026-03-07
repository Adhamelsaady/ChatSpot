namespace ChatSpot.Models.NoSQL;
using MongoDB.Driver;
public class MongoDbContext
{
    private readonly IMongoDatabase _db;
    public MongoDbContext(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionString"]);
        _db = client.GetDatabase(config["MongoDB:DatabaseName"]);
        CreateIndexes();
    }

    public IMongoCollection<MessageDocument> Messages =>
        _db.GetCollection<MessageDocument>("messages");

    public IMongoCollection<ConversationDocument> Conversations =>
        _db.GetCollection<ConversationDocument>("conversations");

    public IMongoCollection<GroupChatMetaDocument> GroupChatMeta =>
        _db.GetCollection<GroupChatMetaDocument>("group_chat_meta");
    private void CreateIndexes()
    {
        Messages.Indexes.CreateOne(new CreateIndexModel<MessageDocument>(
            Builders<MessageDocument>.IndexKeys
                .Ascending(m => m.SenderId)
                .Ascending(m => m.ReceiverId)
                .Descending(m => m.Timestamp)));

        Messages.Indexes.CreateOne(new CreateIndexModel<MessageDocument>(
            Builders<MessageDocument>.IndexKeys
                .Ascending(m => m.GroupId)
                .Descending(m => m.Timestamp)));

        Conversations.Indexes.CreateOne(new CreateIndexModel<ConversationDocument>(
            Builders<ConversationDocument>.IndexKeys
                .Ascending(c => c.Participants)));

        GroupChatMeta.Indexes.CreateOne(new CreateIndexModel<GroupChatMetaDocument>(
            Builders<GroupChatMetaDocument>.IndexKeys
                .Ascending(g => g.GroupId),
            new CreateIndexOptions { Unique = true }));
    }
}