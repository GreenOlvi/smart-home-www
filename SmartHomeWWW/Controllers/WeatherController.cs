using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHomeCore.Domain;
using SmartHomeCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartHomeWWW.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        public WeatherController(ILogger<WeatherController> logger, IDbContextFactory<SmartHomeDbContext> dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        private readonly ILogger<WeatherController> _logger;
        private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;

        [HttpGet("current")]
        public async Task<string> GetCurrent()
        {
            using var db = _dbContextFactory.CreateDbContext();
            var current = await db.WeatherCaches.OrderByDescending(w => w.Timestamp).FirstOrDefaultAsync();
            return current.Data;
        }

        [HttpPost]
        public async Task Post([FromBody] object value)
        {
            _logger.LogInformation("Received new weather data");
            var newCache = new WeatherCache()
            {
                Id = Guid.NewGuid(),
                Data = value.ToString(),
                Timestamp = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(1),
                Name = "current",
            };

            using var db = _dbContextFactory.CreateDbContext();
            db.WeatherCaches.Add(newCache);
            var expired = db.WeatherCaches.Where(w => w.Expires <= DateTime.UtcNow);
            db.WeatherCaches.RemoveRange(expired);

            await db.SaveChangesAsync();
        }
    }
}
