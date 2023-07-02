using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Messages.Commands;

public record TasmotaRequestPowerStateCommand : IMessage
{
    public string DeviceName { get; init; } = string.Empty;
}
