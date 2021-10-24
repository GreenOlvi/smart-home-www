using Microsoft.EntityFrameworkCore;
using SmartHomeCore.Domain;

namespace SmartHomeCore.Infrastructure
{
    public class SmartHomeDbContext : DbContext
    {
        public SmartHomeDbContext(DbContextOptions<SmartHomeDbContext> dbContextOptions) : base(dbContextOptions)
        {
        }

        public DbSet<Sensor> Sensors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}
