using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Messages.Events;

public readonly record struct TasmotaPowerUpdateEvent : IMessage
{
    public IMessage? ParentEvent { get; init; }
    public string DeviceName { get; init; }
    public string PowerState { get; init; }
}
