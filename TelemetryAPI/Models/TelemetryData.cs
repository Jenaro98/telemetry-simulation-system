using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TelemetryAPI.Models;

public class TelemetryData
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string DeviceId { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double Pressure { get; set; }
    public double BatteryLevel { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    
    [BsonElement("AdditionalData")]
    [BsonIgnoreIfNull]
    public Dictionary<string, object>? AdditionalData { get; set; }
}
