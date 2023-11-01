using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using SmartHomeWWW.Core.Domain.Repositories;
using SmartHomeWWW.Core.Infrastructure;
using System.Text.Json;

namespace SmartHomeWWW.Server.Repositories;

public class WeatherReportRepository(ILogger<WeatherReportRepository> logger, SmartHomeDbContext dbContext) : IWeatherReportRepository
{
    private static readonly TimeSpan ExpireTime = TimeSpan.FromDays(1);
    private const string CurrentName = "current";

    private readonly ILogger<WeatherReportRepository> _logger = logger;
    private readonly SmartHomeDbContext _db = dbContext;
    private readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true };

    public Task<WeatherReport?> GetCurrentWeatherReport() => GetCurrentWeatherReport(DateTime.MinValue);

    public async Task<WeatherReport?> GetCurrentWeatherReport(DateTime after)
    {
        var current = await _db.WeatherCaches
            .Where(w => w.Timestamp > after && w.Name == CurrentName)
            .OrderByDescending(w => w.Timestamp)
            .FirstOrDefaultAsync();

        if (current == null)
        {
            return null;
        }

        if (!TryDeserialize(current.Data, out var report))
        {
            _logger.LogWarning("Could not parse weather data id={Id}", current.Id);
            return null;
        }

        return report;
    }

    public Task<WeatherReport?> GetWeatherReport(string type) => throw new NotImplementedException();

    public async Task SaveWeatherReport(WeatherReport weather, string type = CurrentName)
    {
        var timestamp = weather.Current.Timestamp;

        var weatherEntity = await _db.WeatherCaches.FirstOrDefaultAsync(w => w.Name == type && w.Timestamp == timestamp);
        if (weatherEntity is null)
        {
            weatherEntity = new WeatherCache()
            {
                Id = Guid.NewGuid(),
                Data = Serialize(weather),
                Timestamp = timestamp,
                Expires = timestamp + ExpireTime,
                Name = type,
            };
            _db.WeatherCaches.Add(weatherEntity);
            _logger.LogDebug("Saved new weather report");
        }
        else
        {
            weatherEntity.Data = JsonSerializer.Serialize(weather);
            _db.WeatherCaches.Update(weatherEntity);
            _logger.LogDebug("Updated existing weather report");
        }

        await CleanExpired();
    }

    private Task<int> CleanExpired(CancellationToken cancellationToken = default) =>
        _db.WeatherCaches.Where(w => w.Expires <= DateTime.UtcNow).ExecuteDeleteAsync(cancellationToken);

    private bool TryDeserialize(string value, out WeatherReport weatherReport)
    {
        var report = JsonSerializer.Deserialize<WeatherReport?>(value, _serializerOptions);
        if (report is null)
        {
            weatherReport = new WeatherReport();
            return false;
        }

        weatherReport = report.Value;
        return true;
    }

    private string Serialize(WeatherReport weatherReport) => JsonSerializer.Serialize(weatherReport, _serializerOptions);
}
