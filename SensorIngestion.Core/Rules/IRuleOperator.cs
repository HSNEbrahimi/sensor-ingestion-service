namespace SensorIngestion.Core.Rules;

public interface IRuleOperator
{
    string Name { get; }
}

public interface IStatelessOperator : IRuleOperator
{
    bool IsSatisfied(Entities.Reading reading, Entities.Rule rule, out string reason);
}

public interface ISustainedOperator : IRuleOperator
{
    IReadOnlyList<Entities.Alert> EvaluateSequence(
        IReadOnlyList<Entities.Reading> readingsInEventTimeOrder,
        Entities.Rule rule,
        TimeSpan cooldown,
        DateTime dataEnd);
}