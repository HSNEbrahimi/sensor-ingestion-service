using SensorIngestion.Core.Entities;

namespace SensorIngestion.Core.Rules.Operators;

public class SustainedAboveOperator : ISustainedOperator
{
    public string Name => "SustainedAbove";

    public IReadOnlyList<Alert> EvaluateSequence(
        IReadOnlyList<Reading> readingsInEventTimeOrder,
        Rule rule,
        TimeSpan cooldown,
        DateTime dataEnd)
    {
        var alerts = new List<Alert>();
        if (rule.Threshold is null || rule.DurationSeconds is null)
            throw new InvalidOperationException($"Rule {rule.Id} missing threshold/durationSeconds");

        var threshold = rule.Threshold.Value;
        var duration = TimeSpan.FromSeconds(rule.DurationSeconds.Value);

        DateTime? windowStart = null;
        double peak = double.MinValue;
        DateTime? lastAlertEnd = null;

        void CloseWindow(DateTime endTs)
        {
            if (windowStart is null) return;
            var span = endTs - windowStart.Value;
            if (span >= duration)
            {
                bool suppressed = lastAlertEnd is not null
                                  && windowStart.Value < lastAlertEnd.Value + cooldown;
                if (!suppressed)
                {
                    alerts.Add(new Alert
                    {
                        RuleId = rule.Id,
                        DeviceId = readingsInEventTimeOrder[0].DeviceId,
                        Metric = rule.Metric,
                        StartTs = windowStart.Value,
                        EndTs = endTs,
                        PeakValue = peak
                    });
                    lastAlertEnd = endTs;
                }
            }
            windowStart = null;
            peak = double.MinValue;
        }

        foreach (var r in readingsInEventTimeOrder)
        {
            if (r.Value > threshold)
            {
                if (windowStart is null)
                {
                    windowStart = r.Ts;
                    peak = r.Value;
                }
                else if (r.Value > peak)
                {
                    peak = r.Value;
                }
            }
            else
            {
                CloseWindow(r.Ts);
            }
        }

        if (windowStart is not null)
            CloseWindow(dataEnd);

        return alerts;
    }
}