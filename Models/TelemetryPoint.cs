using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RacingTelemetry.Models;

public class TelemetryPoint
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public int LapId { get; set; }
    public int Gear { get; set; }
    public int RPM { get; set; }
    public int Speed { get; set; }
    public int ThrottlePercentage { get; set; }
    public int BrakePercentage { get; set; }
    public int DRS { get; set; }
    public DateTime Timestamp { get; set; }
    public Coordinates? Position { get; set; }
}

public class Coordinates
{
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public int PositionZ { get; set; }
}