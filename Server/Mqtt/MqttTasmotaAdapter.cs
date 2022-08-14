using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Server.Messages.Events;
using System.Text.Json;

namespace SmartHomeWWW.Server.Mqtt;

public sealed partial class MqttTasmotaAdapter : IOrchestratorJob,
    IMessageHandler<MqttMessageReceivedEvent>,
    IMessageHandler<TasmotaRequestPowerStateCommand>
{
    private readonly ILogger<MqttTasmotaAdapter> _logger;
    private readonly IMessageBus _bus;
    private readonly List<(Func<string, bool> Match, Func<MqttMessageReceivedEvent, Task> Handler)> TopicHandlers = new();

    public MqttTasmotaAdapter(ILogger<MqttTasmotaAdapter> logger, IMessageBus bus)
    {
        _logger = logger;
        _bus = bus;

        TopicHandlers.Add((s => s.StartsWith("stat/", StringComparison.InvariantCultureIgnoreCase), MqttStatMessage));
        TopicHandlers.Add((s => s.StartsWith("tasmota/discovery/", StringComparison.InvariantCultureIgnoreCase), MqttTasmotaDiscoveryMessage));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public Task Start(CancellationToken cancellationToken)
    {
        _bus.Subscribe<MqttMessageReceivedEvent>(this);
        _bus.Subscribe<TasmotaRequestPowerStateCommand>(this);

        _bus.Publish(new MqttSubscribeToTopicCommand { Topic = "stat/+/POWER" });
        _bus.Publish(new MqttSubscribeToTopicCommand { Topic = "tasmota/discovery/+/config" });

        return Task.CompletedTask;
    }

    public Task Stop(CancellationToken cancellationToken)
    {
        _bus.Unsubscribe<MqttMessageReceivedEvent>(this);
        return Task.CompletedTask;
    }

    public Task Handle(MqttMessageReceivedEvent message) =>
        Task.WhenAll(TopicHandlers.Where(h => h.Match(message.Topic)).Select(h => h.Handler(message)));


    private Task MqttStatMessage(MqttMessageReceivedEvent message)
    {
        var parts = message.Topic.Split('/');
        var (device, kind) = (parts[1], parts[2]);

        if (kind == "POWER")
        {
            _logger.LogInformation("{Dev} changed power to {Payload}", device, message.Payload);
            _bus.Publish(new TasmotaPowerUpdateEvent
            {
                ParentEvent = message,
                DeviceName = device,
                PowerState = message.Payload,
            });
        }
        return Task.CompletedTask;
    }

    private Task MqttTasmotaDiscoveryMessage(MqttMessageReceivedEvent message)
    {
        var data = JsonSerializer.Deserialize<TasmotaDiscoveryMessage>(message.Payload);
        if (data is not null)
        {
            _logger.LogInformation("Discovered device {Name}, ip: {Ip}, mac: {Mac}, topic: {Topic}", data.DeviceName, data.Ip, data.Mac, data.Topic);
        }
        return Task.CompletedTask;
    }

    public Task Handle(TasmotaRequestPowerStateCommand message)
    {
        _bus.Publish(new MqttPublishMessageCommand
        {
            Topic = $"cmnd/{message.DeviceName}/POWER",
        });
        return Task.CompletedTask;
    }
}
