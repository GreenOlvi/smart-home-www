﻿using SmartHomeWWW.Core.Utils;
using System.Text.Json.Serialization;

namespace SmartHomeWWW.Core.Domain.OpenWeatherMaps;

public readonly record struct CurrentWeather
{
    public CurrentWeather()
    {
    }

    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    [JsonPropertyName("dt")]
    public DateTime Timestamp { get; init; } = DateTime.UnixEpoch;

    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    public DateTime Sunrise { get; init; } = DateTime.UnixEpoch;

    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    public DateTime Sunset { get; init; } = DateTime.UnixEpoch;

    [JsonPropertyName("temp")]
    public float Temperature { get; init; }

    [JsonPropertyName("feels_like")]
    public float FeelsLike { get; init; }

    public int Pressure { get; init; }

    public int Humidity { get; init; }

    [JsonPropertyName("dew_point")]
    public float DewPoint { get; init; }

    public float Uvi { get; init; }

    public int Clouds { get; init; }

    public int Visibility { get; init; }

    [JsonPropertyName("wind_speed")]
    public float WindSpeed { get; init; }

    [JsonPropertyName("wind_deg")]
    public int WindDegree { get; init; }

    public IReadOnlyCollection<WeatherDescription> Weather { get; init; } = [];
}
