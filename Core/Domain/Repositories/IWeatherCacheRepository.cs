using SmartHomeWWW.Core.Domain.OpenWeatherMaps;

namespace SmartHomeWWW.Core.Domain.Repositories;
public interface IWeatherReportRepository
{
    Task<WeatherReport?> GetCurrentWeatherReport();
    Task<WeatherReport?> GetCurrentWeatherReport(DateTime after);
    Task<WeatherReport?> GetWeatherReport(string type = "current");
    Task SaveWeatherReport(WeatherReport weather, string type = "current");
}
