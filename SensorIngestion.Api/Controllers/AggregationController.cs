using Microsoft.AspNetCore.Mvc;
using SensorIngestion.Api.Models;
using SensorIngestion.Core.Abstractions;

namespace SensorIngestion.Api.Controllers;

[ApiController]
[Route("api/aggregation")]
public class AggregationController : ControllerBase
{
    private readonly IReadingRepository _readings;

    public AggregationController(IReadingRepository readings) => _readings = readings;

    [HttpGet]
    public async Task<ActionResult<List<AggregationBucket>>> Get(
        [FromQuery] string deviceId,
        [FromQuery] string metric,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int bucketSizeSeconds,
        CancellationToken ct)
    {
        if (bucketSizeSeconds <= 0)
            return BadRequest("bucketSizeSeconds must be > 0");
        if (to <= from)
            return BadRequest("to must be greater than from");

        var readings = await _readings.QueryAcceptableAsync(deviceId, metric, from, to, ct);

        // bucket boundaries relative to `from`
        var buckets = new Dictionary<DateTime, List<double>>();
        foreach (var r in readings)
        {
            var offsetSeconds = (r.Ts - from).TotalSeconds;
            var bucketIndex = (long)(offsetSeconds / bucketSizeSeconds);
            var bucketStart = from.AddSeconds(bucketIndex * bucketSizeSeconds);
            if (!buckets.TryGetValue(bucketStart, out var list))
            {
                list = new List<double>();
                buckets[bucketStart] = list;
            }
            list.Add(r.Value);
        }

        var result = buckets
            .OrderBy(kv => kv.Key)
            .Select(kv => new AggregationBucket
            {
                BucketStart = kv.Key,
                Count = kv.Value.Count,
                Average = kv.Value.Average(),
                Min = kv.Value.Min(),
                Max = kv.Value.Max()
            })
            .ToList();

        return Ok(result);
    }
}