using SensorIngestion.Core.Entities;
using SensorIngestion.Core.Rules;
using SensorIngestion.Core.Rules.Operators;
using Xunit;

namespace SensorIngestion.Tests;

public class RuleEngineTests
{
    private static RuleEngine BuildEngine() => new(new IRuleOperator[]
    {
        new GreaterThanOperator(), new GreaterThanOrEqualOperator(),
        new LessThanOperator(), new LessThanOrEqualOperator(),
        new EqualOperator(), new BetweenOperator(), new SustainedAboveOperator()
    });

    [Fact]
    public void Reading_Is_Unacceptable_When_Stateless_Rule_Violated()
    {
        var engine = BuildEngine();
        var readings = new List<Reading>
        {
            new() { Id = 1, DeviceId = "D1", Metric = "pressure", Ts = DateTime.UtcNow, Value = 5, Seq = 1 }
        };
        var rules = new List<Rule>
        {
            new() { Id = "r1", Enabled = true, Metric = "pressure",
                    Operator = "GreaterThan", Threshold = 10 }
        };

        var result = engine.Evaluate(readings, rules, TimeSpan.FromMinutes(5), DateTime.UtcNow);
        Assert.Equal(0, result.AcceptableCount);
        Assert.Equal(1, result.UnacceptableCount);
        Assert.Equal(1, result.ViolationCount);
        Assert.False(readings[0].IsAcceptable);
    }

    [Fact]
    public void Reading_Is_Acceptable_When_No_Rule_Applies()
    {
        var engine = BuildEngine();
        var readings = new List<Reading>
        {
            new() { Id = 1, DeviceId = "D1", Metric = "humidity", Ts = DateTime.UtcNow, Value = 50, Seq = 1 }
        };
        var rules = new List<Rule>
        {
            new() { Id = "r1", Enabled = true, Metric = "pressure",
                    Operator = "GreaterThan", Threshold = 10 }
        };
        var result = engine.Evaluate(readings, rules, TimeSpan.FromMinutes(5), DateTime.UtcNow);
        Assert.Equal(1, result.AcceptableCount);
        Assert.Equal(0, result.UnacceptableCount);
        Assert.True(readings[0].IsAcceptable);
    }
}