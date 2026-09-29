using SensorIngestion.Core.Entities;

namespace SensorIngestion.Core.Rules;

public class RuleEvaluationResult
{
    public List<RuleResult> Results { get; } = new();
    public List<Alert> Alerts { get; } = new();
    public int EvaluationsPerformed { get; set; }
    public int AcceptableCount { get; set; }
    public int UnacceptableCount { get; set; }
    public int ViolationCount { get; set; }
}