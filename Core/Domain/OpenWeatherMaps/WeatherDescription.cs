using System.Text.Json.Serialization;

namespace SmartHomeWWW.Core.Domain.OpenWeatherMaps;

public readonly record struct WeatherDescription
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("main")]
    public string Main { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; }

    [JsonPropertyName("icon")]
    public string Icon { get; init; }
}
