using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Messages.Commands;

public readonly record struct MqttSubscribeToTopicCommand : IMessage
{
    public string Topic { get; init; }
}
