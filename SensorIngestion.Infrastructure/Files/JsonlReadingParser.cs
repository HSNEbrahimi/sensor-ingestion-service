using System.Globalization;
using System.Text.Json;
using SensorIngestion.Core.Entities;
using SensorIngestion.Core.Processing;

namespace SensorIngestion.Infrastructure.Files;

public class JsonlReadingParser : IReadingParser
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Reading Parse(string line)
    {
        using var doc = JsonDocument.Parse(line);
        var root = doc.RootElement;

        var deviceId = root.GetProperty("deviceId").GetString() ?? "";
        var metric = root.GetProperty("metric").GetString() ?? "";
        var tsRaw = root.GetProperty("ts").GetString()
                    ?? throw new InvalidReadingException("ts missing");
        var ts = DateTime.Parse(tsRaw, CultureInfo.InvariantCulture,
                                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        var value = root.GetProperty("value").GetDouble();
        var seq = root.GetProperty("seq").GetInt32();

        return new Reading
        {
            DeviceId = deviceId,
            Metric = metric,
            Ts = ts,
            Value = value,
            Seq = seq
        };
    }
}