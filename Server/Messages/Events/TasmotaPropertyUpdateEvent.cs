namespace SmartHomeWWW.Server.Messages.Events;

public readonly record struct TasmotaPropertyUpdateEvent : IMessage
{
    public IMessage? ParentEvent { get; init; }
    public string DeviceId { get; init; }
    public string PropertyName { get; init; }
    public string Value { get; init; }
}
