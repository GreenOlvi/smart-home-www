using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Infrastructure;

namespace SmartHomeWWW.Core.Tests;

internal static class SmartHomeDbTestContextFactory
{
    public static async Task<SmartHomeDbContext> CreateInMemoryAsync(string? name = null)
    {
        var dbName = name ?? Guid.NewGuid().ToString();
        var builder = new SqliteConnectionStringBuilder()
        {
            DataSource = dbName,
            Mode = SqliteOpenMode.Memory,
            Cache = SqliteCacheMode.Shared,
        };

        var opts = new DbContextOptionsBuilder<SmartHomeDbContext>()
            .UseSqlite(builder.ConnectionString, o => o.MigrationsAssembly("SmartHomeWWW.Server"))
            .EnableSensitiveDataLogging()
            .Options;

        var db = new SmartHomeDbContext(opts);
        await db.Database.OpenConnectionAsync();
        await db.Database.MigrateAsync();
        return db;
    }
}
