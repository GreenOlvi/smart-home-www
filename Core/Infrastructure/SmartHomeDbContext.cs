using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Entities;

namespace SmartHomeWWW.Core.Infrastructure;

public sealed class SmartHomeDbContext : DbContext
{
    public SmartHomeDbContext(DbContextOptions<SmartHomeDbContext> dbContextOptions) : base(dbContextOptions)
    {
    }

    public DbSet<CacheStoreEntry> CacheEntries { get; init; } = null!;
    public DbSet<RelayEntry> Relays { get; init; } = null!;
    public DbSet<Sensor> Sensors { get; init; } = null!;
    public DbSet<SettingEntry> Settings { get; init; } = null!;
    public DbSet<TelegramUser> TelegramUsers { get; init; } = null!;
    public DbSet<WeatherCache> WeatherCaches { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<RelayEntry>()
            .Property(e => e.Config)
            .HasConversion<JsonConverter<object>>();
    }
}
