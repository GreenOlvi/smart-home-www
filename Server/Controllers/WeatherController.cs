using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Events;

namespace SmartHomeWWW.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
    public WeatherController(ILogger<WeatherController> logger, IDbContextFactory<SmartHomeDbContext> dbContextFactory, IMessageBus bus)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _bus = bus;
    }

    private static readonly TimeSpan ExpireTime = TimeSpan.FromDays(1);

    private readonly ILogger<WeatherController> _logger;
    private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;
    private readonly IMessageBus _bus;

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(long after = 0)
    {
        var afterDt = DateTimeOffset.FromUnixTimeSeconds(after).DateTime;

        using var db = _dbContextFactory.CreateDbContext();
        var current = await db.WeatherCaches
            .Where(w => w.Timestamp > afterDt && w.Name == "current")
            .OrderByDescending(w => w.Timestamp)
            .FirstOrDefaultAsync();

        if (current is null)
        {
            return NotFound();
        }

        var report = JsonSerializer.Deserialize<WeatherReport?>(current.Data);
        if (report is null)
        {
            _logger.LogWarning("Could not parse weather data id={Id}", current.Id);
            return NoContent();
        }

        return Ok(report.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetWeather(Guid id)
    {
        using var db = _dbContextFactory.CreateDbContext();
        var weather = await db.WeatherCaches.FindAsync(id);
        if (weather is null)
        {
            return NotFound();
        }

        var report = JsonSerializer.Deserialize<WeatherReport?>(weather.Data);
        if (report is null)
        {
            _logger.LogWarning("Could not parse weather data id={Id}", weather.Id);
            return NoContent();
        }

        return Ok(report.Value);
    }

    [HttpPost("{type=current}")]
    public async Task<IActionResult> PostWeather(string type, [FromBody] WeatherReport value)
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

        _bus.Publish(new WeatherUpdatedEvent { Type = weather.Name, Weather = value });

        return CreatedAtAction(nameof(GetWeather), new { id = weather.Id }, weather);
    }
}
