using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Entities;

namespace SmartHomeWWW.Core.Infrastructure
{
    public class SmartHomeDbContext : DbContext
    {
        public SmartHomeDbContext(DbContextOptions<SmartHomeDbContext> dbContextOptions) : base(dbContextOptions)
        {
        }

        public DbSet<RelayEntry> Relays { get; init; }
        public DbSet<Sensor> Sensors { get; init; }
        public DbSet<WeatherCache> WeatherCaches { get; init; }
    }
}
