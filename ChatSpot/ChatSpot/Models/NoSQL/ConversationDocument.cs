using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatSpot.Models.NoSQL;

public class ConversationDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public List<string> Participants { get; set; } = new();

    public string LastMessage { get; set; } = string.Empty;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public Dictionary<string, int> UnreadCount { get; set; } = new();
}