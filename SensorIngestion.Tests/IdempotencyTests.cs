using Microsoft.EntityFrameworkCore;
using SensorIngestion.Core.Entities;
using SensorIngestion.Infrastructure.Persistence;
using Xunit;

namespace SensorIngestion.Tests;

public class IdempotencyTests
{
    [Fact]
    public async Task Inserting_Same_Reading_Twice_Fails_Unique_Index()
    {
        using var db = TestDbFactory.Create();
        var t0 = new DateTime(2025, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        var r1 = new Reading { DeviceId = "D1", Metric = "m", Ts = t0, Value = 10, Seq = 1 };
        var r2 = new Reading { DeviceId = "D1", Metric = "m", Ts = t0, Value = 99, Seq = 1 };

        db.Readings.Add(r1);
        await db.SaveChangesAsync();

        db.Readings.Add(r2);
        await Assert.ThrowsAnyAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Inserting_Same_Alert_Twice_Fails_Unique_Index()
    {
        using var db = TestDbFactory.Create();
        var t0 = new DateTime(2025, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        db.Alerts.Add(new Alert { RuleId = "r1", DeviceId = "D1", Metric = "m", StartTs = t0, EndTs = t0.AddSeconds(30), PeakValue = 85 });
        await db.SaveChangesAsync();

        db.Alerts.Add(new Alert { RuleId = "r1", DeviceId = "D1", Metric = "m", StartTs = t0, EndTs = t0.AddSeconds(50), PeakValue = 99 });
        await Assert.ThrowsAnyAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
}