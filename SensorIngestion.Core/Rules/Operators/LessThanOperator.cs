using SensorIngestion.Core.Entities;

namespace SensorIngestion.Core.Rules.Operators;

public class LessThanOperator : IStatelessOperator
{
    public string Name => "LessThan";

    public bool IsSatisfied(Reading reading, Rule rule, out string reason)
    {
        var t = rule.Threshold ?? throw new InvalidOperationException($"Rule {rule.Id} missing threshold");
        if (reading.Value < t) { reason = string.Empty; return true; }
        reason = $"Value {reading.Value} is not less than {t}";
        return false;
    }
}