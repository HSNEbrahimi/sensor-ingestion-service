using Microsoft.Extensions.Options;
using SensorIngestion.Api.Options;
using SensorIngestion.Core.Processing;

namespace SensorIngestion.Api.Services;

public class FileProcessingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<IngestionOptions> _options;
    private readonly ILogger<FileProcessingBackgroundService> _logger;

    public FileProcessingBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<IngestionOptions> options,
        ILogger<FileProcessingBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opt = _options.Value;
        if (!opt.RunOnStartup)
        {
            _logger.LogInformation("RunOnStartup is false; skipping file processing.");
            return;
        }

        _logger.LogInformation("Starting file processing...");
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IFileProcessor>();

            var readings = Path.Combine(AppContext.BaseDirectory, opt.ReadingsPath);
            var rules = Path.Combine(AppContext.BaseDirectory, opt.RulesPath);

            await processor.ProcessAsync(
                readings,
                rules,
                TimeSpan.FromMinutes(opt.CooldownMinutes),
                stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File processing failed.");
        }
    }
}