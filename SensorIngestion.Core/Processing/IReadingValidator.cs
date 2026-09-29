using SensorIngestion.Core.Entities;

namespace SensorIngestion.Core.Processing;

public interface IReadingValidator
{
    bool IsValid(Reading reading, out string reason);
}

public class ReadingValidator : IReadingValidator
{
    public bool IsValid(Reading reading, out string reason)
    {
        if (string.IsNullOrWhiteSpace(reading.DeviceId)) { reason = "deviceId empty"; return false; }
        if (string.IsNullOrWhiteSpace(reading.Metric)) { reason = "metric empty"; return false; }
        if (reading.Ts == default) { reason = "ts invalid"; return false; }
        if (double.IsNaN(reading.Value) || double.IsInfinity(reading.Value))
        { reason = "value NaN/Infinity"; return false; }
        reason = string.Empty;
        return true;
    }
}