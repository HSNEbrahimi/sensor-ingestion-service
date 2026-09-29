using SensorIngestion.Core.Entities;

namespace SensorIngestion.Core.Rules.Operators;

public class GreaterThanOrEqualOperator : IStatelessOperator
{
    public string Name => "GreaterThanOrEqual";

    public bool IsSatisfied(Reading reading, Rule rule, out string reason)
    {
        var t = rule.Threshold ?? throw new InvalidOperationException($"Rule {rule.Id} missing threshold");
        if (reading.Value >= t) { reason = string.Empty; return true; }
        reason = $"Value {reading.Value} is not greater than or equal to {t}";
        return false;
    }
}