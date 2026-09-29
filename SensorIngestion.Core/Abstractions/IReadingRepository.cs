using SensorIngestion.Core.Entities;

namespace SensorIngestion.Core.Abstractions;

public interface IReadingRepository
{
    Task<HashSet<string>> GetExistingNaturalKeysAsync(CancellationToken ct);
    Task AddRangeAsync(IEnumerable<Reading> readings, CancellationToken ct);
    Task<List<Reading>> QueryAcceptableAsync(
        string deviceId, string metric, DateTime from, DateTime to, CancellationToken ct);
}