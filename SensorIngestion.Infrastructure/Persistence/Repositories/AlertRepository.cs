using Microsoft.EntityFrameworkCore;
using SensorIngestion.Core.Abstractions;
using SensorIngestion.Core.Entities;

namespace SensorIngestion.Infrastructure.Persistence.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _db;
    public AlertRepository(AppDbContext db) => _db = db;

    public async Task<HashSet<string>> GetExistingNaturalKeysAsync(CancellationToken ct)
    {
        var rows = await _db.Alerts
            .Select(a => new { a.RuleId, a.DeviceId, a.Metric, a.StartTs })
            .ToListAsync(ct);
        return rows
            .Select(a => $"{a.RuleId}|{a.DeviceId}|{a.Metric}|{a.StartTs:O}")
            .ToHashSet(StringComparer.Ordinal);
    }

    public async Task AddRangeAsync(IEnumerable<Alert> alerts, CancellationToken ct)
        => await _db.Alerts.AddRangeAsync(alerts, ct);
}