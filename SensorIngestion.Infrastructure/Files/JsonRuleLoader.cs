using System.Text.Json;
using SensorIngestion.Core.Entities;
using SensorIngestion.Core.Processing;

namespace SensorIngestion.Infrastructure.Files;

public class JsonRuleLoader : IRuleLoader
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<Rule>> LoadAsync(string path, CancellationToken ct)
    {
        if (!File.Exists(path)) throw new FileNotFoundException($"Rules file not found: {path}");
        var json = await File.ReadAllTextAsync(path, ct);
        var rules = JsonSerializer.Deserialize<List<Rule>>(json, Opts) ?? new();
        return rules;
    }
}