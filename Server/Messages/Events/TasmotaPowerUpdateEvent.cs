using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Messages.Events;

public record TasmotaPowerUpdateEvent : IMessage
{
    public IMessage? ParentEvent { get; init; }
    public required string DeviceName { get; init; }
    public required string PowerState { get; init; }
}
