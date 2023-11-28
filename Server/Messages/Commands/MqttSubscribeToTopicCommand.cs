using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Messages.Commands;

public record MqttSubscribeToTopicCommand : IMessage
{
    public required string Topic { get; init; }
}
