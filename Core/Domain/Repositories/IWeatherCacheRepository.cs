using SmartHomeWWW.Core.Domain.OpenWeatherMaps;

namespace SmartHomeWWW.Core.Domain.Repositories;
public interface IWeatherReportRepository
{
    public const string CurrentKey = "current";

    Task<WeatherReport?> GetCurrentWeatherReport();
    Task<WeatherReport?> GetCurrentWeatherReport(DateTime after);
    Task<WeatherReport?> GetWeatherReport(string type = CurrentKey);
    Task SaveWeatherReport(WeatherReport weather, string type = CurrentKey);
}
