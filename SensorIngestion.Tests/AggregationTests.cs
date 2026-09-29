using SensorIngestion.Core.Entities;
using SensorIngestion.Infrastructure.Persistence;
using SensorIngestion.Infrastructure.Persistence.Repositories;
using Xunit;

namespace SensorIngestion.Tests;

public class AggregationTests
{
    [Fact]
    public async Task Query_Only_Returns_Acceptable_Readings()
    {
        using var db = TestDbFactory.Create();
        var t0 = new DateTime(2025, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        db.Readings.AddRange(
            new Reading { DeviceId = "D1", Metric = "m", Ts = t0, Value = 10, Seq = 1, IsAcceptable = true },
            new Reading { DeviceId = "D1", Metric = "m", Ts = t0.AddSeconds(30), Value = 20, Seq = 2, IsAcceptable = false },
            new Reading { DeviceId = "D1", Metric = "m", Ts = t0.AddSeconds(60), Value = 30, Seq = 3, IsAcceptable = true }
        );
        await db.SaveChangesAsync();

        var repo = new ReadingRepository(db);
        var result = await repo.QueryAcceptableAsync("D1", "m", t0, t0.AddMinutes(5), default);

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, r => r.Value == 20);
    }
}