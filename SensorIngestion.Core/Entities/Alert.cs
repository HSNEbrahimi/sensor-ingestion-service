namespace SensorIngestion.Core.Entities;

public class Alert
{
    public int Id { get; set; }
    public string RuleId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty;
    public DateTime StartTs { get; set; }
    public DateTime EndTs { get; set; }
    public double PeakValue { get; set; }

    public string NaturalKey => $"{RuleId}|{DeviceId}|{Metric}|{StartTs:O}";
}