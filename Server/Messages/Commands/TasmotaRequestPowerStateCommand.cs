using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Messages.Commands;

public readonly record struct TasmotaRequestPowerStateCommand : IMessage
{
    public string DeviceName { get; init; }
}
