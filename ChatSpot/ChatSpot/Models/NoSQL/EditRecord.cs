using MongoDB.Bson.Serialization.Attributes;

namespace ChatSpot.Models.NoSQL;

public class EditRecord
{
    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("editedAt")]
    public DateTime EditedAt { get; set; } = DateTime.UtcNow;
}