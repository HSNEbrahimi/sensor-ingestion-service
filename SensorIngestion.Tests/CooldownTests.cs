using SensorIngestion.Core.Entities;
using SensorIngestion.Core.Rules.Operators;
using Xunit;

namespace SensorIngestion.Tests;

public class CooldownTests
{
    [Fact]
    public void Two_Episodes_Within_Cooldown_Produce_One_Alert()
    {
        var t0 = new DateTime(2025, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        var readings = new List<Reading>
        {
            new() { DeviceId = "P1", Metric = "temperature", Ts = t0,                    Value = 85, Seq = 1 },
            new() { DeviceId = "P1", Metric = "temperature", Ts = t0.AddSeconds(40),     Value = 70, Seq = 2 },
            new() { DeviceId = "P1", Metric = "temperature", Ts = t0.AddSeconds(60),     Value = 85, Seq = 3 },
            new() { DeviceId = "P1", Metric = "temperature", Ts = t0.AddSeconds(120),    Value = 70, Seq = 4 }
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
        Assert.Single(alerts);
    }
}