using MongoDB.Bson.Serialization.Attributes;

namespace OnboardSystem.Models;
public class Sensor
{
    public Sensor(Guid dataId, int id, double value, string unit, long timestamp)
    {
        this.dataId = dataId.ToString();
        this.id = id;
        this.value = value;
        this.unit = unit;
        this.timestamp = timestamp;
    }

    [BsonId]
    public string dataId { get; set; }

    public int id { get; set; }

    public double value { get; set; }

    public string unit { get; set; } = null!;

    public long timestamp { get; set; }

    public double avgValue { get; set; }
}
