using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Messages.Events;

public readonly record struct MqttMessageReceivedEvent : IMessage
{
    public string Topic { get; init; }
    public string Payload { get; init; }
}
