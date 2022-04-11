namespace SmartHomeWWW.Core.Domain.OpenWeatherMaps
{
    public record WeatherDescription
    {
        public int Id { get; init; }
        public string Main { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Icon { get; init; } = string.Empty;
    }

}
