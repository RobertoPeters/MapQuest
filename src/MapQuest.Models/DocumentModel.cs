using System.Text.Json.Serialization;

namespace MapQuest.Models;

public class DocumentModel
{
    public string Id { get; set; } = null!;

    public string? UserId { get; set; }

    public long? Lat { get; set; }

    public long? Lon { get; set; }

    public string? QuestId { get; set; }

    public DateTime InsertedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string NewId()
    {
        return Guid.NewGuid().ToString();
    }

    public string ToData()
    {
        return System.Text.Json.JsonSerializer.Serialize(this, this.GetType());
    }

    public static T FromData<T>(string data) where T : DocumentModel, new()
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<T>(data)!;
        return result;
    }
}
