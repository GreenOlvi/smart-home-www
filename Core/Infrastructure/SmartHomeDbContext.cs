using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Entities;

namespace SmartHomeWWW.Core.Infrastructure;

public sealed class SmartHomeDbContext(DbContextOptions<SmartHomeDbContext> dbContextOptions) : DbContext(dbContextOptions)
{
    public DbSet<CacheStoreEntry> CacheEntries { get; init; } = null!;
    public DbSet<RelayEntry> Relays { get; init; } = null!;
    public DbSet<Sensor> Sensors { get; init; } = null!;
    public DbSet<SettingEntry> Settings { get; init; } = null!;
    public DbSet<TelegramUser> TelegramUsers { get; init; } = null!;
    public DbSet<WeatherCache> WeatherCaches { get; init; } = null!;
}
