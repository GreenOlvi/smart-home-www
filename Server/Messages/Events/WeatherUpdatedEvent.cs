using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Messages.Events;

public class WeatherUpdatedEvent : IMessage
{
    public string Type { get; init; } = string.Empty;
    public WeatherReport Weather { get; init; }
}
