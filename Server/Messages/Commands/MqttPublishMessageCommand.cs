using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Messages.Commands;

public readonly record struct MqttPublishMessageCommand : IMessage
{
    public string Topic { get; init; }
    public string Payload { get; init; }
}
