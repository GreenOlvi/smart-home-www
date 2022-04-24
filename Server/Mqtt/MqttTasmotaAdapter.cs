using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Server.Messages.Events;

namespace SmartHomeWWW.Server.Mqtt
{
    public sealed class MqttTasmotaAdapter : IOrchestratorJob,
        IMessageHandler<MqttMessageReceivedEvent>,
        IMessageHandler<TasmotaRequestPowerStateCommand>
    {
        public MqttTasmotaAdapter(ILogger<MqttTasmotaAdapter> logger, IMessageBus bus)
        {
            _logger = logger;
            _bus = bus;
        }

        private readonly ILogger<MqttTasmotaAdapter> _logger;
        private readonly IMessageBus _bus;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public Task Start(CancellationToken cancellationToken)
        {
            _bus.Subscribe<MqttMessageReceivedEvent>(this);
            _bus.Subscribe<TasmotaRequestPowerStateCommand>(this);

            _bus.Publish(new MqttSubscribeToTopicCommand { Topic = "stat/+/POWER" });

            return Task.CompletedTask;
        }

        public Task Stop(CancellationToken cancellationToken)
        {
            _bus.Unsubscribe<MqttMessageReceivedEvent>(this);
            return Task.CompletedTask;
        }

        public Task Handle(MqttMessageReceivedEvent message)
        {
            var parts = message.Topic.Split('/');
            return parts[0] switch
            {
                "stat" => MqttStatMessage(parts[1], parts[2], message.Payload, message),
                _ => Task.CompletedTask,
            };
        }

        private Task MqttStatMessage(string device, string kind, string payload, IMessage message)
        {
            if (kind == "POWER")
            {
                _logger.LogInformation("{dev} changed power to {payload}", device, payload);
                _bus.Publish(new TasmotaPowerUpdateEvent
                {
                    ParentEvent = message,
                    DeviceName = device,
                    PowerState = payload,
                });
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
}
