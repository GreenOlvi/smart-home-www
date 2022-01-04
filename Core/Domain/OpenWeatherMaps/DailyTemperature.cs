using System.Text.Json.Serialization;

namespace SmartHomeWWW.Core.Domain.OpenWeatherMaps
{
    public record DailyTemperature
    {
        public float Day { get; init; }
        public float Min { get; init; }
        public float Max { get; init; }
        public float Night { get; init; }

        [JsonPropertyName("eve")]
        public float Evening { get; init; }

        [JsonPropertyName("morn")]
        public float Morning { get; init; }
    }
}