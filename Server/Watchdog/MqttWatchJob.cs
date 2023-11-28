using MassTransit;
using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Server.Messages.Events;

namespace SmartHomeWWW.Server.Watchdog;

public sealed class MqttWatchJob(ILogger<MqttWatchJob> logger, string topic, TimeSpan timeout, Action onTimeout, IMessageBus messsageBus, IPublishEndpoint publisher)
    : WatchJob(timeout, onTimeout), IConsumer<MqttMessageReceivedEvent>
{
    private readonly ILogger<MqttWatchJob> _logger = logger;
    private readonly string _topic = topic;
    private readonly IMessageBus _messageBus = messsageBus;
    private readonly IPublishEndpoint _publisher = publisher;

    public async override Task Init()
    {
        await base.Init();
        _messageBus.Publish(new MqttSubscribeToTopicCommand { Topic = _topic });
        await _publisher.Publish<MqttSubscribeToTopicCommand>(new() { Topic = _topic });
    }

    public Task Consume(ConsumeContext<MqttMessageReceivedEvent> context)
    {
        if (context.Message.Topic == _topic)
        {
            Reset();
            _logger.LogDebug("Watch job reset with Mqtt message: '{Topic}'", context.Message.Topic);
        }
        return Task.CompletedTask;
    }
}
