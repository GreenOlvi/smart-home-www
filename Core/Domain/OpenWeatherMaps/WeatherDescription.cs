namespace SmartHomeWWW.Core.Domain.OpenWeatherMaps;

public readonly record struct WeatherDescription
{
    public int Id { get; init; }
    public string Main { get; init; }
    public string Description { get; init; }
    public string Icon { get; init; }
}
