namespace SensorIngestion.Core.Entities;

public class Reading
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty;
    public DateTime Ts { get; set; }
    public double Value { get; set; }
    public int Seq { get; set; }
    public bool IsAcceptable { get; set; }

    public string NaturalKey => $"{DeviceId}|{Metric}|{Ts:O}|{Seq}";
}