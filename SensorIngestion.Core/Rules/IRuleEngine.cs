using SensorIngestion.Core.Entities;

namespace SensorIngestion.Core.Rules;

public interface IRuleEngine
{
    RuleEvaluationResult Evaluate(
        IReadOnlyList<Reading> readings,
        IReadOnlyList<Rule> rules,
        TimeSpan cooldown,
        DateTime dataEnd);
}