using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Server.Messages.Events;

namespace SmartHomeWWW.Server.Watchdog;

public sealed class MqttWatchJob(ILogger<MqttWatchJob> logger, string topic, TimeSpan timeout, Action onTimeout, IMessageBus bus)
    : WatchJob(timeout, onTimeout), IMessageHandler<MqttMessageReceivedEvent>, IAsyncDisposable
{
    private readonly ILogger<MqttWatchJob> _logger = logger;
    private readonly string _topic = topic;
    private readonly IMessageBus _messageBus = bus;

    public override void Init()
    {
        base.Init();
        _messageBus.Subscribe(this);
        _messageBus.Publish(new MqttSubscribeToTopicCommand { Topic = _topic });
    }

    public Task Handle(MqttMessageReceivedEvent message)
    {
        if (message.Topic == _topic)
        {
            Reset();
            _logger.LogDebug("Watch job reset with Mqtt message: '{Topic}'", message.Topic);
        }
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _messageBus.Unsubscribe(this);
        return ValueTask.CompletedTask;
    }
}
