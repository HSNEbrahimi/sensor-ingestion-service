using SensorIngestion.Core.Entities;

namespace SensorIngestion.Core.Abstractions;

public interface IRuleRepository
{
    Task AddRangeAsync(IEnumerable<Rule> rules, CancellationToken ct);
    Task<List<Rule>> GetAllAsync(CancellationToken ct);
}