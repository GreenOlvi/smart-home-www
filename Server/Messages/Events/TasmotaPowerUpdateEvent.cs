namespace SmartHomeWWW.Server.Messages.Events
{
    public record TasmotaPowerUpdateEvent : IMessage
    {
        public IMessage? ParentEvent { get; init; }
        public string DeviceName { get; init; } = string.Empty;
        public string PowerState { get; init; } = string.Empty;
    }
}
