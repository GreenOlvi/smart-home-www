namespace SmartHomeWWW.Server.Messages.Commands
{
    public record MqttSubscribeToTopicCommand : IMessage
    {
        public string Topic { get; init; } = string.Empty;
    }
}
