using System.Text.Json.Serialization;

namespace MapQuest.Models;

public class DocumentModel
{
    [JsonIgnore]
    public string Id { get; set; } = null!;

    [JsonIgnore]
    public string? UserId { get; set; }

    [JsonIgnore]
    public long? Lat { get; set; }

    [JsonIgnore]
    public long? Lon { get; set; }

    [JsonIgnore]
    public string? QuestId { get; set; }

    [JsonIgnore]
    public DateTime InsertedAt { get; set; }

    [JsonIgnore]
    public DateTime? UpdatedAt { get; set; }


    public string NewId()
    {
        return Guid.NewGuid().ToString();
    }

    public string ToData()
    {
        return System.Text.Json.JsonSerializer.Serialize(this, this.GetType());
    }

    public static T FromData<T>(string id, string? userId, long? lat, long? lon, string? questId, DateTime insertedAt, DateTime? updatedAt, string data) where T : DocumentModel, new()
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<T>(data)!;
        result.Id = id;
        result.UserId = userId;
        result.Lat = lat;
        result.Lon = lon;
        result.QuestId = questId;
        result.InsertedAt = insertedAt;
        result.UpdatedAt = updatedAt;
        return result;
    }
}
