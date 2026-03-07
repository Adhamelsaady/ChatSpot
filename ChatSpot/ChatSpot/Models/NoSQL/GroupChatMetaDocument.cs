using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatSpot.Models.NoSQL;

public class GroupChatMetaDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("groupId")]
    public string GroupId { get; set; } = string.Empty;

    [BsonElement("lastMessage")]
    public string LastMessage { get; set; } = string.Empty;

    [BsonElement("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    [BsonElement("unreadCount")]
    public Dictionary<string, int> UnreadCount { get; set; } = new();

    [BsonElement("pinnedMessageIds")]
    public List<string> PinnedMessageIds { get; set; } = new();
}