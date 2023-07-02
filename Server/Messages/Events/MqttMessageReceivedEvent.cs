using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Messages.Events;

public record MqttMessageReceivedEvent : IMessage
{
    public string Topic { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
}
