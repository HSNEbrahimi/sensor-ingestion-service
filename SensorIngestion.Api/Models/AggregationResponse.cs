namespace SensorIngestion.Api.Models;

public class AggregationBucket
{
    public DateTime BucketStart { get; set; }
    public int Count { get; set; }
    public double Average { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
}