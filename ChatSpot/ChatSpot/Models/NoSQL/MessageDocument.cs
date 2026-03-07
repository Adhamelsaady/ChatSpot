using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatSpot.Models.NoSQL;

public class MessageDocument
{
    [BsonId][BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("senderId")]
    public string SenderId { get; set; } = string.Empty;        // → users.id (PG)

    [BsonElement("receiverId")]
    public string? ReceiverId { get; set; }                     // → users.id (PG) — DM only

    [BsonElement("groupId")]
    public string? GroupId { get; set; }                        // → groups.id (PG) — group only

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("isRead")]
    public bool IsRead { get; set; } = false;

    // text | image | video | audio | file
    [BsonElement("messageType")]
    public string MessageType { get; set; } = "text";

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    [BsonElement("isEdited")]
    public bool IsEdited { get; set; } = false;

    [BsonElement("editedAt")]
    public DateTime? EditedAt { get; set; }

    [BsonElement("editHistory")]
    public List<EditRecord> EditHistory { get; set; } = new();


    [BsonElement("replyToId")]
    public string? ReplyToId { get; set; }

    [BsonElement("replyToPreview")]
    public string? ReplyToPreview { get; set; }
}