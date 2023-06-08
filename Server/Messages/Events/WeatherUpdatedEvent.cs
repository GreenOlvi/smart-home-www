using SmartHomeWWW.Core.Domain.OpenWeatherMaps;

namespace SmartHomeWWW.Server.Messages.Events;

public class WeatherUpdatedEvent : IMessage
{
    public string Type { get; init; } = string.Empty;
    public WeatherReport Weather { get; init; }
}
