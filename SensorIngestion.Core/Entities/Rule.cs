namespace SensorIngestion.Core.Entities;

public class Rule
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string Metric { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
    public string Operator { get; set; } = string.Empty;
    public double? Threshold { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
    public int? DurationSeconds { get; set; }
}