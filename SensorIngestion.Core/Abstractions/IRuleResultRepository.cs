using SensorIngestion.Core.Entities;

namespace SensorIngestion.Core.Abstractions;

public interface IRuleResultRepository
{
    Task AddRangeAsync(IEnumerable<RuleResult> results, CancellationToken ct);
}