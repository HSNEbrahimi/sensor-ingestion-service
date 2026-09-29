using Microsoft.EntityFrameworkCore;
using SensorIngestion.Api.Options;
using SensorIngestion.Api.Services;
using SensorIngestion.Core.Abstractions;
using SensorIngestion.Core.Processing;
using SensorIngestion.Core.Rules;
using SensorIngestion.Core.Rules.Operators;
using SensorIngestion.Infrastructure.Files;
using SensorIngestion.Infrastructure.Persistence;
using SensorIngestion.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<IngestionOptions>(
    builder.Configuration.GetSection(IngestionOptions.SectionName));

// --- EF Core ---
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// --- Repositories ---
builder.Services.AddScoped<IReadingRepository, ReadingRepository>();
builder.Services.AddScoped<IRuleRepository, RuleRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IRuleResultRepository, RuleResultRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// --- File I/O ---
builder.Services.AddSingleton<IReadingParser, JsonlReadingParser>();
builder.Services.AddSingleton<IReadingValidator, ReadingValidator>();
builder.Services.AddSingleton<IRuleLoader, JsonRuleLoader>();

// --- Rule operators ---
builder.Services.AddSingleton<IRuleOperator, GreaterThanOperator>();
builder.Services.AddSingleton<IRuleOperator, GreaterThanOrEqualOperator>();
builder.Services.AddSingleton<IRuleOperator, LessThanOperator>();
builder.Services.AddSingleton<IRuleOperator, LessThanOrEqualOperator>();
builder.Services.AddSingleton<IRuleOperator, EqualOperator>();
builder.Services.AddSingleton<IRuleOperator, BetweenOperator>();
builder.Services.AddSingleton<IRuleOperator, SustainedAboveOperator>();

// --- Rule engine + processor ---
builder.Services.AddSingleton<IRuleEngine, RuleEngine>();
builder.Services.AddScoped<IFileProcessor, FileProcessor>();

// --- Background service ---
builder.Services.AddHostedService<FileProcessingBackgroundService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// migrate DB at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();