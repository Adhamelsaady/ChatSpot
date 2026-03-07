using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatSpot.Models.NoSQL;

public class ConversationDocument
{
    [BsonId][BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("participants")]
    public List<string> Participants { get; set; } = new();

    [BsonElement("lastMessage")]
    public string LastMessage { get; set; } = string.Empty;

    [BsonElement("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    [BsonElement("unreadCount")]
    public Dictionary<string, int> UnreadCount { get; set; } = new();
}