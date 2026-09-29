using Microsoft.EntityFrameworkCore;
using SensorIngestion.Core.Abstractions;
using SensorIngestion.Core.Entities;

namespace SensorIngestion.Infrastructure.Persistence.Repositories;

public class RuleRepository : IRuleRepository
{
    private readonly AppDbContext _db;
    public RuleRepository(AppDbContext db) => _db = db;

    public async Task AddRangeAsync(IEnumerable<Rule> rules, CancellationToken ct)
        => await _db.Rules.AddRangeAsync(rules, ct);

    public Task<List<Rule>> GetAllAsync(CancellationToken ct)
        => _db.Rules.AsNoTracking().ToListAsync(ct);
}