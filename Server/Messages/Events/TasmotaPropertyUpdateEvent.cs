using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Messages.Events;

public record TasmotaPropertyUpdateEvent : IMessage
{
    public IMessage? ParentEvent { get; init; }
    public required string DeviceId { get; init; }
    public required string PropertyName { get; init; }
    public required string Value { get; init; }
}
