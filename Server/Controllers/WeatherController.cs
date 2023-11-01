using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using SmartHomeWWW.Core.Domain.Repositories;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.Messages.Events;
using System.Text.Json;

namespace SmartHomeWWW.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WeatherController(ILogger<WeatherController> logger, IDbContextFactory<SmartHomeDbContext> dbContextFactory, IMessageBus bus, SmartHomeDbContext db,
    IWeatherReportRepository weatherReportRepository) : ControllerBase
{
    private readonly ILogger<WeatherController> _logger = logger;
    private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory = dbContextFactory;
    private readonly IMessageBus _bus = bus;
    private readonly SmartHomeDbContext _db = db;
    private readonly IWeatherReportRepository _weatherReportRepository = weatherReportRepository;

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(long after = 0)
    {
        var afterDt = DateTimeOffset.FromUnixTimeSeconds(after).DateTime;
        var report = await _weatherReportRepository.GetCurrentWeatherReport(afterDt);
        return report is null ? NotFound() : Ok(report.Value);
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

        await _weatherReportRepository.SaveWeatherReport(value, type);
        await _db.SaveChangesAsync();

        _bus.Publish(new WeatherUpdatedEvent { Type = type, Weather = value });

        return Ok();
    }
}
