using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SensorIngestion.Infrastructure.Persistence;

namespace SensorIngestion.Tests;

public static class TestDbFactory
{
    public static AppDbContext Create()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conn)
            .Options;
        var db = new AppDbContext(opts);
        db.Database.EnsureCreated();
        return db;
    }
}