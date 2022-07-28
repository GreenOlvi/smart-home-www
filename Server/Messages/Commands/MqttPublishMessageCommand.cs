namespace SmartHomeWWW.Server.Messages.Commands;

public record MqttPublishMessageCommand : IMessage
{
    public string Topic { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
}
