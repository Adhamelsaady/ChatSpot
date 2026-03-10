using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatSpot.Models.NoSQL;

public class GroupChatMetaDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string GroupId { get; set; } = string.Empty;

    public string LastMessage { get; set; } = string.Empty;
    
    public string LastReadMessageId { get; set; } = string.Empty;

    public string LastMessageId { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    [BsonElement("unreadCount")]
    public Dictionary<string, int> UnreadCount { get; set; } = new();

}