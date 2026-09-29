namespace SensorIngestion.Core.Entities;

public class RuleResult
{
    public int Id { get; set; }
    public int ReadingId { get; set; }
    public Reading Reading { get; set; } = null!;
    public string RuleId { get; set; } = string.Empty;
    public bool IsViolation { get; set; }
    public string Reason { get; set; } = string.Empty;
}