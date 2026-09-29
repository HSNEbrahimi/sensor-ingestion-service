using Microsoft.EntityFrameworkCore;
using SensorIngestion.Core.Entities;

namespace SensorIngestion.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Reading> Readings => Set<Reading>();
    public DbSet<Rule> Rules => Set<Rule>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<RuleResult> RuleResults => Set<RuleResult>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Reading>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DeviceId).IsRequired();
            e.Property(x => x.Metric).IsRequired();
            e.HasIndex(x => new { x.DeviceId, x.Metric, x.Ts, x.Seq }).IsUnique();
            e.Ignore(x => x.NaturalKey);
        });

        b.Entity<Rule>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
        });

        b.Entity<Alert>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.RuleId, x.DeviceId, x.Metric, x.StartTs }).IsUnique();
            e.Ignore(x => x.NaturalKey);
        });

        b.Entity<RuleResult>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Reading).WithMany().HasForeignKey(x => x.ReadingId);
            e.HasIndex(x => new { x.ReadingId, x.RuleId });
        });
    }
}