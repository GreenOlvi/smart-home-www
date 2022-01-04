using System;
using System.Text.Json.Serialization;

namespace SmartHomeWWW.Core.Domain.OpenWeatherMaps
{
    public class WeatherReport
    {
        [JsonPropertyName("lat")]
        public float Latitude { get; init; }

        [JsonPropertyName("lon")]
        public float Longitude { get; init; }

        public string Timezone { get; init; }

        [JsonPropertyName("timezone_offset")]
        public long TimezoneOffset { get; init; }

        public CurrentWeather Current { get; init; }

        public MinutelyWeather[] Minutely { get; init; } = Array.Empty<MinutelyWeather>();

        public HourlyWeather[] Hourly { get; init; } = Array.Empty<HourlyWeather>();

        public DailyWeather[] Daily { get; init; } = Array.Empty<DailyWeather>();

        public WeatherAlert[] Alerts { get; init; } = Array.Empty<WeatherAlert>();
    }
}
