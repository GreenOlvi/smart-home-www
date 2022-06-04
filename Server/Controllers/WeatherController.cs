using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using SmartHomeWWW.Core.Infrastructure;
using System.Text.Json;

namespace SmartHomeWWW.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
    public WeatherController(ILogger<WeatherController> logger, IDbContextFactory<SmartHomeDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    private readonly static TimeSpan ExpireTime = TimeSpan.FromDays(1);

    private readonly ILogger<WeatherController> _logger;
    private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;

    [HttpGet("current")]
    public async Task<ActionResult<WeatherReport>> GetCurrent(long after = 0)
    {
        var afterDt = DateTimeOffset.FromUnixTimeSeconds(after).DateTime;

        using var db = _dbContextFactory.CreateDbContext();
        var current = await db.WeatherCaches
            .Where(w => w.Timestamp > afterDt)
            .OrderByDescending(w => w.Timestamp)
            .FirstOrDefaultAsync();

        if (current?.Data == null)
        {
            return NoContent();
        }

        var report = JsonSerializer.Deserialize<WeatherReport>(current.Data);
        return report is null
            ? NoContent()
            : (ActionResult<WeatherReport>)report;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WeatherReport>> GetWeather(Guid id)
    {
        using var db = _dbContextFactory.CreateDbContext();
        var weather = await db.WeatherCaches.FindAsync(id);
        if (weather is null)
        {
            return NotFound();
        }

        var report = JsonSerializer.Deserialize<WeatherReport>(weather.Data);
        return report is null
            ? NoContent()
            : (ActionResult<WeatherReport>)report;
    }

    [HttpPost("{type=current}")]
    public async Task<ActionResult> PostWeather(string type, [FromBody] WeatherReport value)
    {
        _logger.LogInformation("Received new weather data");
        var timestamp = value.Current.Timestamp;

        using var db = _dbContextFactory.CreateDbContext();

        var weather = await db.WeatherCaches.FirstOrDefaultAsync(w => w.Name == type && w.Timestamp == timestamp);
        if (weather is null)
        {
            weather = new WeatherCache()
            {
                Id = Guid.NewGuid(),
                Data = JsonSerializer.Serialize(value),
                Timestamp = timestamp,
                Expires = timestamp + ExpireTime,
                Name = type,
            };
            db.WeatherCaches.Add(weather);
        }
        else
        {
            weather.Data = JsonSerializer.Serialize(value);
            db.WeatherCaches.Update(weather);
        }

        var expired = db.WeatherCaches.Where(w => w.Expires <= DateTime.UtcNow);
        db.WeatherCaches.RemoveRange(expired);

        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWeather), new { id = weather.Id }, weather);
    }
}

