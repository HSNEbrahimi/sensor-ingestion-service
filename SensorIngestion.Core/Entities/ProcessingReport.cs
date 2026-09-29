namespace SensorIngestion.Core.Entities;

public class ProcessingReport
{
    public int TotalLinesRead { get; set; }
    public int ParsedReadings { get; set; }
    public int StoredReadings { get; set; }
    public int DuplicatesRemoved { get; set; }
    public int InvalidRecordsRejected { get; set; }
    public int RulesLoaded { get; set; }
    public int RuleEvaluationsPerformed { get; set; }
    public int AcceptableReadings { get; set; }
    public int UnacceptableReadings { get; set; }
    public int RuleViolations { get; set; }
    public int AlertsGenerated { get; set; }

    public override string ToString() =>
        $"""
        ============ PROCESSING REPORT ============
        Total lines read:              {TotalLinesRead}
        Parsed readings:               {ParsedReadings}
        Stored readings:               {StoredReadings}
        Duplicates removed:            {DuplicatesRemoved}
        Invalid records rejected:      {InvalidRecordsRejected}
        Rules loaded:                  {RulesLoaded}
        Rule evaluations performed:    {RuleEvaluationsPerformed}
        Acceptable readings:           {AcceptableReadings}
        Unacceptable readings:         {UnacceptableReadings}
        Rule violations:               {RuleViolations}
        Alerts generated:              {AlertsGenerated}
        ===========================================
        """;
}