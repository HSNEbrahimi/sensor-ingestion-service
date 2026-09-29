using SensorIngestion.Core.Entities;

namespace SensorIngestion.Core.Rules.Operators;

public class EqualOperator : IStatelessOperator
{
    public string Name => "Equal";

    public bool IsSatisfied(Reading reading, Rule rule, out string reason)
    {
        var t = rule.Threshold ?? throw new InvalidOperationException($"Rule {rule.Id} missing threshold");
        if (Math.Abs(reading.Value - t) < 1e-9) { reason = string.Empty; return true; }
        reason = $"Value {reading.Value} is not equal to {t}";
        return false;
    }
}