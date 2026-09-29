using SensorIngestion.Core.Entities;

namespace SensorIngestion.Core.Rules;

public class RuleEngine : IRuleEngine
{
    private readonly IReadOnlyDictionary<string, IRuleOperator> _operators;

    public RuleEngine(IEnumerable<IRuleOperator> operators)
    {
        _operators = operators.ToDictionary(o => o.Name, StringComparer.OrdinalIgnoreCase);
    }

    public RuleEvaluationResult Evaluate(
        IReadOnlyList<Reading> readings,
        IReadOnlyList<Rule> rules,
        TimeSpan cooldown,
        DateTime dataEnd)
    {
        var result = new RuleEvaluationResult();
        var enabledRules = rules.Where(r => r.Enabled).ToList();

        // map reading id -> violations
        var violationsByReading = new Dictionary<int, List<RuleResult>>();
        foreach (var r in readings) violationsByReading[r.Id] = new List<RuleResult>();

        // ---- Stateless rules ----
        foreach (var rule in enabledRules)
        {
            if (!_operators.TryGetValue(rule.Operator, out var op)) continue;
            if (op is not IStatelessOperator stateless) continue;

            foreach (var reading in readings)
            {
                if (!Applies(rule, reading)) continue;
                result.EvaluationsPerformed++;

                var satisfied = stateless.IsSatisfied(reading, rule, out var reason);
                var rr = new RuleResult
                {
                    ReadingId = reading.Id,
                    RuleId = rule.Id,
                    IsViolation = !satisfied,
                    Reason = satisfied ? "OK" : reason
                };
                result.Results.Add(rr);
                if (!satisfied)
                {
                    violationsByReading[reading.Id].Add(rr);
                    result.ViolationCount++;
                }
            }
        }

        // ---- Sustained rules ----
        foreach (var rule in enabledRules)
        {
            if (!_operators.TryGetValue(rule.Operator, out var op)) continue;
            if (op is not ISustainedOperator sustained) continue;

            var applicable = readings.Where(r => Applies(rule, r)).ToList();
            if (applicable.Count == 0) continue;

            foreach (var group in applicable.GroupBy(r => r.DeviceId))
            {
                var ordered = group.OrderBy(r => r.Ts).ThenBy(r => r.Seq).ToList();
                result.EvaluationsPerformed += ordered.Count;

                var alerts = sustained.EvaluateSequence(ordered, rule, cooldown, dataEnd);
                foreach (var alert in alerts)
                {
                    result.Alerts.Add(alert);

                    // mark every reading in [StartTs, EndTs] as violation of this rule
                    foreach (var r in ordered.Where(x => x.Ts >= alert.StartTs && x.Ts <= alert.EndTs))
                    {
                        var rr = new RuleResult
                        {
                            ReadingId = r.Id,
                            RuleId = rule.Id,
                            IsViolation = true,
                            Reason = $"SustainedAbove violation window {alert.StartTs:O} → {alert.EndTs:O}"
                        };
                        result.Results.Add(rr);
                        violationsByReading[r.Id].Add(rr);
                        result.ViolationCount++;
                    }
                }
            }
        }

        // ---- Classify ----
        foreach (var r in readings)
        {
            var hasViolation = violationsByReading[r.Id].Count > 0;
            r.IsAcceptable = !hasViolation;
            if (hasViolation) result.UnacceptableCount++;
            else result.AcceptableCount++;
        }

        return result;
    }

    private static bool Applies(Rule rule, Reading reading)
    {
        if (!string.Equals(rule.Metric, reading.Metric, StringComparison.OrdinalIgnoreCase))
            return false;
        if (rule.DeviceId is not null &&
            !string.Equals(rule.DeviceId, reading.DeviceId, StringComparison.OrdinalIgnoreCase))
            return false;
        return true;
    }
}