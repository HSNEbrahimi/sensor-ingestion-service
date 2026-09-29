using SensorIngestion.Core.Entities;
using SensorIngestion.Core.Rules.Operators;
using Xunit;

namespace SensorIngestion.Tests;

public class SustainedAboveTests
{
    [Fact]
    public void Out_Of_Order_Readings_Produce_One_Alert()
    {
        var t0 = new DateTime(2025, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        var readings = new List<Reading>
        {
            new() { DeviceId = "P1", Metric = "temperature", Ts = t0.AddSeconds(40), Value = 85, Seq = 3 },
            new() { DeviceId = "P1", Metric = "temperature", Ts = t0.AddSeconds(0),  Value = 85, Seq = 1 },
            new() { DeviceId = "P1", Metric = "temperature", Ts = t0.AddSeconds(20), Value = 86, Seq = 2 },
            new() { DeviceId = "P1", Metric = "temperature", Ts = t0.AddSeconds(60), Value = 70, Seq = 4 }
        };

        var rule = new Rule
        {
            Id = "r1",
            Name = "OH",
            Enabled = true,
            Metric = "temperature",
            DeviceId = "P1",
            Operator = "SustainedAbove",
            Threshold = 80,
            DurationSeconds = 30
        };

        var op = new SustainedAboveOperator();
        var ordered = readings.OrderBy(r => r.Ts).ToList();
        var alerts = op.EvaluateSequence(ordered, rule, TimeSpan.FromMinutes(5), ordered.Last().Ts);

        Assert.Single(alerts);
        Assert.Equal(t0, alerts[0].StartTs);
        Assert.Equal(t0.AddSeconds(60), alerts[0].EndTs);
    }

    [Fact]
    public void Short_Episode_Does_Not_Alert()
    {
        var t0 = new DateTime(2025, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        var readings = new List<Reading>
        {
            new() { DeviceId = "P1", Metric = "temperature", Ts = t0, Value = 85, Seq = 1 },
            new() { DeviceId = "P1", Metric = "temperature", Ts = t0.AddSeconds(10), Value = 70, Seq = 2 }
        };
        var rule = new Rule
        {
            Id = "r1",
            Enabled = true,
            Metric = "temperature",
            DeviceId = "P1",
            Operator = "SustainedAbove",
            Threshold = 80,
            DurationSeconds = 30
        };
        var op = new SustainedAboveOperator();
        var alerts = op.EvaluateSequence(readings, rule, TimeSpan.FromMinutes(5), readings.Last().Ts);
        Assert.Empty(alerts);
    }
}