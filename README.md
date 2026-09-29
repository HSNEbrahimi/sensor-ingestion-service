# Sensor Ingestion Service

## Run
1. `dotnet restore`
2. Ensure `readings.jsonl` and `rules.json` are in the Api output folder (Copy if newer).
3. `dotnet run --project src/SensorIngestion.Api`
4. Swagger: `/swagger`

## Test
`dotnet test`

## Architecture
- **Core**: domain entities, rule engine, operators, file processing orchestration.
- **Infrastructure**: EF Core (SQLite), JSON parsers.
- **Api**: Web API + BackgroundService.

Domain logic does NOT depend on EF Core or ASP.NET.

## Deduplication
A reading is duplicate when `(deviceId, metric, ts, seq)` matches. **First-wins** (file order). DB-level unique index enforces idempotency.

## Ordering & SustainedAbove
Readings are sorted by `(ts, seq)` (event time). Streaming state per `(deviceId, metric)`:
- Window opens when value > threshold.
- Window closes when value <= threshold (or end of data).
- Episode qualifies if `end - start >= durationSeconds`.
- Late arrivals are handled by sorting before evaluation — batch sort, not streaming. This is O(n log n) and fine for ~2150 readings.

## Cooldown
5 minutes by default. After an alert for `(ruleId, deviceId, metric)`, no new alert until `previous.EndTs + cooldown`. Implemented in `SustainedAboveOperator`.

## Classification
- Acceptable: satisfies all applicable enabled rules.
- Unacceptable: at least one violation. Rule ID + reason stored in `RuleResults`.
- No applicable rule → acceptable.
- Invalid/duplicate readings never reach evaluation.

## Aggregation
`GET /api/aggregation?deviceId=&metric=&from=&to=&bucketSizeSeconds=`
Buckets are aligned to `from`. Only acceptable readings. Empty buckets are omitted.

## Idempotency
- Readings: unique index on `(deviceId, metric, ts, seq)`.
- Alerts: unique index on `(ruleId, deviceId, metric, startTs)`.
- Rule results: only inserted for newly-inserted readings.

## Extensibility
Add a new operator:
1. Implement `IStatelessOperator` or `ISustainedOperator`.
2. Register in DI (`builder.Services.AddSingleton<IRuleOperator, MyOp>()`).
No changes to `RuleEngine`.

## AI Disclosure
Parts of this code were generated with AI assistance and reviewed by the author.