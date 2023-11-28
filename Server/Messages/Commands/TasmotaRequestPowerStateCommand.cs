using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Messages.Commands;

public record TasmotaRequestPowerStateCommand : IMessage
{
    public required string DeviceName { get; init; }
}
