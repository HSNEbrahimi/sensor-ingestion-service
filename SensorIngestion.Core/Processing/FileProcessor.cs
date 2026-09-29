using Microsoft.Extensions.Logging;
using SensorIngestion.Core.Abstractions;
using SensorIngestion.Core.Entities;
using SensorIngestion.Core.Rules;

namespace SensorIngestion.Core.Processing;

public class FileProcessor : IFileProcessor
{
    private readonly IReadingParser _parser;
    private readonly IReadingValidator _validator;
    private readonly IRuleLoader _ruleLoader;
    private readonly IRuleEngine _ruleEngine;
    private readonly IReadingRepository _readings;
    private readonly IRuleRepository _rules;
    private readonly IAlertRepository _alerts;
    private readonly IRuleResultRepository _ruleResults;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<FileProcessor> _logger;

    public FileProcessor(
        IReadingParser parser,
        IReadingValidator validator,
        IRuleLoader ruleLoader,
        IRuleEngine ruleEngine,
        IReadingRepository readings,
        IRuleRepository rules,
        IAlertRepository alerts,
        IRuleResultRepository ruleResults,
        IUnitOfWork uow,
        ILogger<FileProcessor> logger)
    {
        _parser = parser;
        _validator = validator;
        _ruleLoader = ruleLoader;
        _ruleEngine = ruleEngine;
        _readings = readings;
        _rules = rules;
        _alerts = alerts;
        _ruleResults = ruleResults;
        _uow = uow;
        _logger = logger;
    }

    public async Task<ProcessingReport> ProcessAsync(
        string readingsPath,
        string rulesPath,
        TimeSpan cooldown,
        CancellationToken ct)
    {
        var report = new ProcessingReport();

        // ---------- 1. Read lines ----------
        if (!File.Exists(readingsPath))
            throw new FileNotFoundException($"Readings file not found: {readingsPath}");

        var lines = await File.ReadAllLinesAsync(readingsPath, ct);
        report.TotalLinesRead = lines.Length;
        _logger.LogInformation("Read {Count} lines from {Path}", lines.Length, readingsPath);

        // ---------- 2. Parse + validate ----------
        var parsed = new List<Reading>();
        foreach (var (line, idx) in lines.Select((l, i) => (l, i)))
        {
            if (string.IsNullOrWhiteSpace(line)) { report.InvalidRecordsRejected++; continue; }
            try
            {
                var reading = _parser.Parse(line);
                if (_validator.IsValid(reading, out var why))
                {
                    parsed.Add(reading);
                }
                else
                {
                    report.InvalidRecordsRejected++;
                    _logger.LogWarning("Invalid reading at line {Line}: {Reason}", idx + 1, why);
                }
            }
            catch (Exception ex)
            {
                report.InvalidRecordsRejected++;
                _logger.LogWarning("Malformed line {Line}: {Message}", idx + 1, ex.Message);
            }
        }
        report.ParsedReadings = parsed.Count;

        // ---------- 3. In-file dedup (first-wins) ----------
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var deduped = new List<Reading>();
        foreach (var r in parsed)
        {
            if (seen.Add(r.NaturalKey)) deduped.Add(r);
            else report.DuplicatesRemoved++;
        }

        // ---------- 4. DB-level dedup (idempotency) ----------
        var existingKeys = await _readings.GetExistingNaturalKeysAsync(ct);
        var newReadings = deduped.Where(r => !existingKeys.Contains(r.NaturalKey)).ToList();
        report.StoredReadings = newReadings.Count;

        // assign deterministic ids in-memory for rule results
        for (int i = 0; i < deduped.Count; i++) deduped[i].Id = i + 1;

        // ---------- 5. Rules ----------
        var rules = await _ruleLoader.LoadAsync(rulesPath, ct);
        report.RulesLoaded = rules.Count;
        _logger.LogInformation("Loaded {Count} rules", rules.Count);

        // persist rules only if new
        var existingRules = await _rules.GetAllAsync(ct);
        var existingRuleIds = existingRules.Select(r => r.Id).ToHashSet(StringComparer.Ordinal);
        var newRules = rules.Where(r => !existingRuleIds.Contains(r.Id)).ToList();
        if (newRules.Count > 0) await _rules.AddRangeAsync(newRules, ct);

        // ---------- 6. Evaluate ----------
        var dataEnd = deduped.Count > 0 ? deduped.Max(r => r.Ts) : DateTime.UtcNow;
        var eval = _ruleEngine.Evaluate(deduped, rules, cooldown, dataEnd);
        report.RuleEvaluationsPerformed = eval.EvaluationsPerformed;
        report.AcceptableReadings = eval.AcceptableCount;
        report.UnacceptableReadings = eval.UnacceptableCount;
        report.RuleViolations = eval.ViolationCount;
        _logger.LogInformation("Evaluated. Acceptable={A}, Unacceptable={U}, Violations={V}",
            eval.AcceptableCount, eval.UnacceptableCount, eval.ViolationCount);

        // ---------- 7. Alerts dedup ----------
        var existingAlertKeys = await _alerts.GetExistingNaturalKeysAsync(ct);
        var newAlerts = eval.Alerts.Where(a => !existingAlertKeys.Contains(a.NaturalKey)).ToList();
        report.AlertsGenerated = newAlerts.Count;

        // ---------- 8. Persist everything in one transaction ----------
        if (newReadings.Count > 0) await _readings.AddRangeAsync(newReadings, ct);

        // remap rule results to real DB ids only for newly inserted readings
        var naturalKeyToDbId = new Dictionary<string, int>(StringComparer.Ordinal);
        // simplest: re-query not needed; we persist rule results only for readings we just inserted
        var newReadingKeys = newReadings.Select(r => r.NaturalKey).ToHashSet(StringComparer.Ordinal);

        var filteredRuleResults = new List<RuleResult>();
        foreach (var rr in eval.Results)
        {
            var reading = deduped.FirstOrDefault(d => d.Id == rr.ReadingId);
            if (reading is null) continue;
            if (!newReadingKeys.Contains(reading.NaturalKey)) continue;
            filteredRuleResults.Add(rr);
        }

        // NOTE: we save readings first so EF assigns DB ids, then re-map.
        await _uow.SaveChangesAsync(ct);

        // After SaveChanges, newReadings[i].Id holds the real DB id.
        var keyToReading = newReadings.ToDictionary(r => r.NaturalKey, r => r, StringComparer.Ordinal);
        foreach (var rr in filteredRuleResults)
        {
            var domainReading = deduped.First(d => d.Id == rr.ReadingId);
            if (keyToReading.TryGetValue(domainReading.NaturalKey, out var dbReading))
                rr.ReadingId = dbReading.Id;
        }

        if (filteredRuleResults.Count > 0) await _ruleResults.AddRangeAsync(filteredRuleResults, ct);
        if (newAlerts.Count > 0) await _alerts.AddRangeAsync(newAlerts, ct);

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Persisted {R} readings, {RR} rule results, {A} alerts",
            newReadings.Count, filteredRuleResults.Count, newAlerts.Count);

        _logger.LogInformation("Processing report:\n{Report}", report);
        return report;
    }
}