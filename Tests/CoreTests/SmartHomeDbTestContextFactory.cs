using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Infrastructure;

namespace SmartHomeWWW.Core.Tests;

internal static class SmartHomeDbTestContextFactory
{
    public static async Task<SmartHomeDbContext> CreateInMemoryAsync()
    {
        var opts = new DbContextOptionsBuilder<SmartHomeDbContext>()
            .UseSqlite("DataSource=:memory:;Mode=ReadWrite", o => o.MigrationsAssembly("SmartHomeWWW.Server"))
            .EnableSensitiveDataLogging()
            .Options;

        var db = new SmartHomeDbContext(opts);
        await db.Database.OpenConnectionAsync();
        await db.Database.MigrateAsync();
        return db;
    }
}
