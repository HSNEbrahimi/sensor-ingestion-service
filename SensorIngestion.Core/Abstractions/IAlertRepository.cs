using SensorIngestion.Core.Entities;

namespace SensorIngestion.Core.Abstractions;

public interface IAlertRepository
{
    Task<HashSet<string>> GetExistingNaturalKeysAsync(CancellationToken ct);
    Task AddRangeAsync(IEnumerable<Alert> alerts, CancellationToken ct);
}