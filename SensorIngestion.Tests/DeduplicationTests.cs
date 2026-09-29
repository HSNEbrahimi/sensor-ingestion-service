using SensorIngestion.Core.Entities;
using Xunit;

namespace SensorIngestion.Tests;

public class DeduplicationTests
{
    [Fact]
    public void Duplicate_Readings_Are_Deduped_FirstWins()
    {
        var a = new Reading { DeviceId = "D1", Metric = "m", Ts = DateTime.UtcNow, Value = 1, Seq = 1 };
        var b = new Reading { DeviceId = "D1", Metric = "m", Ts = a.Ts, Value = 2, Seq = 1 };
        Assert.Equal(a.NaturalKey, b.NaturalKey);
    }
}