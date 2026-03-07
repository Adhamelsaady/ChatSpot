using MongoDB.Bson.Serialization.Attributes;

namespace ChatSpot.Models.NoSQL;

public class EditRecord
{
    public string Content { get; set; } = string.Empty;

    public DateTime EditedAt { get; set; } = DateTime.UtcNow;
}