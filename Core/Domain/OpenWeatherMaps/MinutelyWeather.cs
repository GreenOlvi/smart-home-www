using SmartHomeWWW.Core.Utils;
using System;
using System.Text.Json.Serialization;

namespace SmartHomeWWW.Core.Domain.OpenWeatherMaps
{
    public record MinutelyWeather
    {
        [JsonConverter(typeof(UnixEpochDateTimeConverter))]
        [JsonPropertyName("dt")]
        public DateTime Timestamp { get; init; } = DateTime.UnixEpoch;

        public float Precipitation { get; init; }
    }

}
