using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Server.Messages.Events;

namespace SmartHomeWWW.Server.Mqtt;

public sealed class MqttClientHostedService : IHostedService, IAsyncDisposable,
    IMessageHandler<MqttPublishMessageCommand>,
    IMessageHandler<MqttSubscribeToTopicCommand>
{
    public MqttClientHostedService(ILogger<MqttClientHostedService> logger, IMqttClientFactory clientFactory, IMqttClientOptions options, IMessageBus bus)
    {
        _logger = logger;
        _client = clientFactory.CreateMqttClient();
        _options = options;
        _bus = bus;

        _client.UseConnectedHandler(async e =>
        {
            _logger.LogDebug("Mqtt client connected");

            foreach (var topic in _subscribedTopics)
            {
                await _client.SubscribeAsync(topic);
            }
        });

        _client.UseDisconnectedHandler(e => _logger.LogDebug("Mqtt client disconnected"));
        _client.UseApplicationMessageReceivedHandler(e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            _logger.LogDebug("Mqtt message received:\n{topic}\n{payload}", topic, payload);
            _bus.Publish(new MqttMessageReceivedEvent { Topic = topic, Payload = payload });
        });
    }

    private readonly ILogger<MqttClientHostedService> _logger;
    private readonly IMqttClient _client;
    private readonly IMqttClientOptions _options;
    private readonly IMessageBus _bus;

    private readonly List<string> _subscribedTopics = new ();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Mqtt service started");
        try
        {
            await _client.ConnectAsync(_options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while connecting to mqtt");
            return;
        }

        _bus.Subscribe<MqttPublishMessageCommand>(this);
        _bus.Subscribe<MqttSubscribeToTopicCommand>(this);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _bus.Unsubscribe<MqttPublishMessageCommand>(this);
        _bus.Unsubscribe<MqttSubscribeToTopicCommand>(this);

        _logger.LogInformation("Mqtt service stopped");
        await _client.DisconnectAsync(cancellationToken: cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }

    public Task Handle(MqttPublishMessageCommand message)
    {
        var builder = new MqttApplicationMessageBuilder()
            .WithTopic(message.Topic)
            .WithPayload(message.Payload);
        return _client.PublishAsync(builder.Build());
    }

    public Task Handle(MqttSubscribeToTopicCommand message)
    {
        _subscribedTopics.Add(message.Topic);
        return _client.IsConnected
            ? _client.SubscribeAsync(message.Topic)
            : Task.CompletedTask;
    }
}
