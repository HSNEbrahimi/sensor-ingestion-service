using SensorIngestion.Core.Entities;

namespace SensorIngestion.Core.Rules.Operators;

public class BetweenOperator : IStatelessOperator
{
    public string Name => "Between";

    public bool IsSatisfied(Reading reading, Rule rule, out string reason)
    {
        if (rule.Min is null || rule.Max is null)
            throw new InvalidOperationException($"Rule {rule.Id} missing min/max");
        if (reading.Value >= rule.Min && reading.Value <= rule.Max)
        {
            reason = string.Empty;
            return true;
        }
        reason = $"Value {reading.Value} not between {rule.Min} and {rule.Max}";
        return false;
    }
}