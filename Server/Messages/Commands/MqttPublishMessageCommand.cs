using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Messages.Commands;

public record MqttPublishMessageCommand : IMessage
{
    public required string Topic { get; init; }
    public string? Payload { get; init; }
}
