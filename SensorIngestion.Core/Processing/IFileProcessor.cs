using SensorIngestion.Core.Entities;

namespace SensorIngestion.Core.Processing;

public interface IFileProcessor
{
    Task<ProcessingReport> ProcessAsync(
        string readingsPath,
        string rulesPath,
        TimeSpan cooldown,
        CancellationToken ct);
}