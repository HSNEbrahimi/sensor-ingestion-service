using SensorIngestion.Core.Entities;

namespace SensorIngestion.Core.Rules.Operators;

public class LessThanOrEqualOperator : IStatelessOperator
{
    public string Name => "LessThanOrEqual";

    public bool IsSatisfied(Reading reading, Rule rule, out string reason)
    {
        var t = rule.Threshold ?? throw new InvalidOperationException($"Rule {rule.Id} missing threshold");
        if (reading.Value <= t) { reason = string.Empty; return true; }
        reason = $"Value {reading.Value} is not less than or equal to {t}";
        return false;
    }
}