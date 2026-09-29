using Microsoft.EntityFrameworkCore;
using SensorIngestion.Core.Abstractions;
using SensorIngestion.Core.Entities;

namespace SensorIngestion.Infrastructure.Persistence.Repositories;

public class ReadingRepository : IReadingRepository
{
    private readonly AppDbContext _db;
    public ReadingRepository(AppDbContext db) => _db = db;

    public async Task<HashSet<string>> GetExistingNaturalKeysAsync(CancellationToken ct)
    {
        var rows = await _db.Readings
            .Select(r => new { r.DeviceId, r.Metric, r.Ts, r.Seq })
            .ToListAsync(ct);

        return rows
            .Select(r => $"{r.DeviceId}|{r.Metric}|{r.Ts:O}|{r.Seq}")
            .ToHashSet(StringComparer.Ordinal);
    }

    public async Task AddRangeAsync(IEnumerable<Reading> readings, CancellationToken ct)
        => await _db.Readings.AddRangeAsync(readings, ct);

    public async Task<List<Reading>> QueryAcceptableAsync(
        string deviceId, string metric, DateTime from, DateTime to, CancellationToken ct)
        => await _db.Readings
            .Where(r => r.DeviceId == deviceId
                     && r.Metric == metric
                     && r.Ts >= from
                     && r.Ts < to
                     && r.IsAcceptable)
            .ToListAsync(ct);
}