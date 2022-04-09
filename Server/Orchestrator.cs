using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Server.Messages.Events;

namespace SmartHomeWWW.Server
{
    public class Orchestrator : IHostedService, IAsyncDisposable,
        IMessageHandler<MqttMessageReceivedEvent>
    {
        public Orchestrator(ILogger<Orchestrator> logger, IMessageBus bus)
        {
            _logger = logger;
            _bus = bus;
        }

        private readonly ILogger<Orchestrator> _logger;
        private readonly IMessageBus _bus;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            SubscribeToEvents();

            _bus.Publish(new MqttSubscribeToTopicCommand { Topic = "stat/+/POWER" });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        private void SubscribeToEvents()
        {
            _bus.Subscribe<MqttMessageReceivedEvent>(this);
        }

        public Task Handle(MqttMessageReceivedEvent message)
        {
            var parts = message.Topic.Split('/');
            return parts[0] switch
            {
                "stat" => MqttStatMessage(parts[1], parts[2], message.Payload),
                _ => MqttUnknownTopicType(message.Topic, message.Payload),
            };
        }

        private Task MqttStatMessage(string device, string kind, string payload)
        {
            if (kind == "POWER")
            {
                _logger.LogInformation("{dev} changed power to {payload}", device, payload);
            }
            return Task.CompletedTask;
        }

        private Task MqttUnknownTopicType(string topic, string payload)
        {
            _logger.LogWarning("Unknown MQTT topic: '{topic}'", topic);
            return Task.CompletedTask;
        }
    }
}
