using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Messages.Events;

public record WeatherUpdatedEvent : IMessage
{
    public required string Type { get; init; }
    public WeatherReport Weather { get; init; }
}
