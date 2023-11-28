using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Messages.Events;

public record MqttMessageReceivedEvent : IMessage
{
    public required string Topic { get; init; }
    public required string Payload { get; init; }
}
