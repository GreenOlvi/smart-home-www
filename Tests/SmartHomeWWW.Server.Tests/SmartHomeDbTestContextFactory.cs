using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Infrastructure;

namespace SmartHomeWWW.Server.Tests;

internal static class SmartHomeDbTestContextFactory
{
    public static Task<SmartHomeDbContext> CreateInMemoryAsync(string? name = null)
    {
        var dbName = name ?? Guid.NewGuid().ToString();
        var builder = new SqliteConnectionStringBuilder()
        {
            DataSource = dbName,
            Mode = SqliteOpenMode.Memory,
            Cache = SqliteCacheMode.Shared,
        };
        return CreateAsync(builder.ConnectionString);
    }

    private static async Task<SmartHomeDbContext> CreateAsync(string connectionString)
    {
        var opts = new DbContextOptionsBuilder<SmartHomeDbContext>()
            .UseSqlite(connectionString, o => o.MigrationsAssembly("SmartHomeWWW.Server"))
            .EnableSensitiveDataLogging()
            .Options;

        var db = new SmartHomeDbContext(opts);
        await db.Database.OpenConnectionAsync();
        await db.Database.MigrateAsync();
        return db;
    }
}
