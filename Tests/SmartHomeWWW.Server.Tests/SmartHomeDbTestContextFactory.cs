using Microsoft.Data.Sqlite;
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

    public static SmartHomeDbContext CreateInMemory(string? name = null)
    {
        var dbName = name ?? Guid.NewGuid().ToString();
        var builder = new SqliteConnectionStringBuilder()
        {
            DataSource = dbName,
            Mode = SqliteOpenMode.Memory,
            Cache = SqliteCacheMode.Shared,
        };
        return Create(builder.ConnectionString);
    }

    private static DbContextOptions<SmartHomeDbContext> BuildOptions(string connectionString) =>
        new DbContextOptionsBuilder<SmartHomeDbContext>()
            .UseSqlite(connectionString, o => o.MigrationsAssembly("SmartHomeWWW.Server"))
            .EnableSensitiveDataLogging()
            .Options;

    private static async Task<SmartHomeDbContext> CreateAsync(string connectionString)
    {
        var opts = BuildOptions(connectionString);

        var db = new SmartHomeDbContext(opts);
        await db.Database.OpenConnectionAsync();
        await db.Database.MigrateAsync();
        return db;
    }
    private static SmartHomeDbContext Create(string connectionString)
    {
        var opts = BuildOptions(connectionString);
        var db = new SmartHomeDbContext(opts);
        db.Database.OpenConnection();
        db.Database.Migrate();
        return db;
    }

    public static IDbContextFactory<SmartHomeDbContext> CreateContextFactory(string? name = null)
    {
        var dbName = name ?? Guid.NewGuid().ToString();
        return new MockContextFactory(() => CreateInMemory(dbName));
    }

    public sealed class MockContextFactory : IDbContextFactory<SmartHomeDbContext>
    {
        private readonly Func<SmartHomeDbContext> _factory;

        public MockContextFactory(Func<SmartHomeDbContext> factory)
        {
            _factory = factory;
        }

        public SmartHomeDbContext CreateDbContext() => _factory();
    }
}
