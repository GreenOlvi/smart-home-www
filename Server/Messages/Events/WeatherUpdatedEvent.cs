using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Messages.Events;

public readonly record struct WeatherUpdatedEvent : IMessage
{
    public string Type { get; init; }
    public WeatherReport Weather { get; init; }
}
