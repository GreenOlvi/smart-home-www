using System.Text.Json;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Core.Utils.Functional;
using static SmartHomeWWW.Core.Utils.Functional.Option<System.Text.Json.JsonDocument>;

namespace SmartHomeWWW.Server.Relays.Tasmota;

public sealed class TasmotaMqttClient : ITasmotaClient
{
    public TasmotaMqttClient(ILogger<TasmotaMqttClient> logger, IMessageBus bus, string deviceId)
    {
        _logger = logger;
        _bus = bus;
        _deviceId = deviceId;

        _bus.Publish(new MqttSubscribeToTopicCommand { Topic = $"stat/{deviceId}/+" });
        _logger.LogInformation("Created new MqttClient for '{DeviceId}'", deviceId);
    }

    private readonly ILogger<TasmotaMqttClient> _logger;
    private readonly string _deviceId;
    private readonly IMessageBus _bus;

    public Task<Option<JsonDocument>> ExecuteCommandAsync(string command, string value)
    {
        _bus.Publish(new MqttPublishMessageCommand
        {
            Topic = $"cmnd/{_deviceId}/{command}",
            Payload = value,
        });

        return Task.FromResult<Option<JsonDocument>>(new None());
    }

    public Task<Option<JsonDocument>> GetValueAsync(string command)
    {
        _bus.Publish(new MqttPublishMessageCommand
        {
            Topic = $"cmnd/{_deviceId}/{command}",
        });

        return Task.FromResult<Option<JsonDocument>>(new None());
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    public void Dispose() { }
}
