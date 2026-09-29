using SensorIngestion.Core.Entities;

namespace SensorIngestion.Core.Processing;

public interface IRuleLoader
{
    Task<List<Rule>> LoadAsync(string path, CancellationToken ct);
}