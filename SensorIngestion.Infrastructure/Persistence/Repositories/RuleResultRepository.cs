using SensorIngestion.Core.Abstractions;
using SensorIngestion.Core.Entities;

namespace SensorIngestion.Infrastructure.Persistence.Repositories;

public class RuleResultRepository : IRuleResultRepository
{
    private readonly AppDbContext _db;
    public RuleResultRepository(AppDbContext db) => _db = db;

    public async Task AddRangeAsync(IEnumerable<RuleResult> results, CancellationToken ct)
        => await _db.RuleResults.AddRangeAsync(results, ct);
}