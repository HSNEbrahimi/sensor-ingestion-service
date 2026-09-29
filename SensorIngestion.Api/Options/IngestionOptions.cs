namespace SensorIngestion.Api.Options;

public class IngestionOptions
{
    public const string SectionName = "Ingestion";
    public string ReadingsPath { get; set; } = "readings.jsonl";
    public string RulesPath { get; set; } = "rules.json";
    public int CooldownMinutes { get; set; } = 5;
    public bool RunOnStartup { get; set; } = true;
}