using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using Polly;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Server.Messages.Events;

namespace SmartHomeWWW.Server.Mqtt;

public sealed class MqttClientHostedService : IHostedService, IAsyncDisposable,
    IMessageHandler<MqttPublishMessageCommand>,
    IMessageHandler<MqttSubscribeToTopicCommand>
{
    public MqttClientHostedService(ILogger<MqttClientHostedService> logger, MqttFactory clientFactory, MqttClientOptions options, IMessageBus bus)
    {
        _logger = logger;
        _client = clientFactory.CreateMqttClient();
        _options = options;
        _bus = bus;

        _client.ConnectedAsync += async e =>
        {
            _logger.LogDebug("Mqtt client connected");

            foreach (var topic in _subscribedTopics)
            {
                await _client.SubscribeAsync(topic);
            }

            while (_queuedMessages.TryDequeue(out var message))
            {
                var builder = new MqttApplicationMessageBuilder()
                    .WithTopic(message.Topic)
                    .WithPayload(message.Payload);
                await _client.PublishAsync(builder.Build());
            }
        };

        _client.DisconnectedAsync += e =>
        {
            _logger.LogInformation("Mqtt client disconnected");
            if (_stayConnected)
            {
                StartConnecting();
            }
            return Task.CompletedTask;
        };

        _client.ApplicationMessageReceivedAsync += e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            _logger.LogDebug("Mqtt message received:\n{Topic}\n{Payload}", topic, payload);
            _bus.Publish(new MqttMessageReceivedEvent { Topic = topic, Payload = payload });
            return Task.CompletedTask;
        };

        ConnectPolicy = Policy
            .Handle<MqttCommunicationException>()
            .WaitAndRetryForeverAsync(retryCount => retryCount < 10 ? TimeSpan.FromSeconds(Math.Pow(2, retryCount)) : TimeSpan.FromMinutes(10),
                (ex, timeout) =>
                {
                    _logger.LogWarning("Mqtt connection failed. Retrying in {Timeout}", timeout);
                    _logger.LogDebug(ex, "Mqtt connection failed");
                });
    }

    private readonly ILogger<MqttClientHostedService> _logger;
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;
    private readonly IMessageBus _bus;
    private bool _stayConnected;
    private bool _connecting;
    private readonly AsyncPolicy ConnectPolicy;

    private Task? _connectTask;

    private readonly List<string> _subscribedTopics = new();
    private readonly Queue<MqttPublishMessageCommand> _queuedMessages = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _stayConnected = true;
        _logger.LogInformation("Mqtt service started");

        StartConnecting();

        _bus.Subscribe<MqttPublishMessageCommand>(this);
        _bus.Subscribe<MqttSubscribeToTopicCommand>(this);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _stayConnected = false;
        _bus.Unsubscribe<MqttPublishMessageCommand>(this);
        _bus.Unsubscribe<MqttSubscribeToTopicCommand>(this);

        _logger.LogInformation("Mqtt service stopped");
        await _client.DisconnectAsync(new MqttClientDisconnectOptions(), cancellationToken: cancellationToken);
    }

    private async Task<bool> TryConnect(CancellationToken cancellationToken)
    {
        var result = await ConnectPolicy.ExecuteAndCaptureAsync(ct => _client.ConnectAsync(_options, ct), cancellationToken);

        if (result.Outcome == OutcomeType.Successful)
        {
            return true;
        }

        return false;
    }

    private void StartConnecting()
    {
        if (_connecting)
        {
            return;
        }

        _connecting = true;
        _connectTask = Task.Run(async () =>
        {
            var success = await TryConnect(CancellationToken.None);
            if (success)
            {
                _connecting = false;
            }
        });
    }

    public ValueTask DisposeAsync()
    {
        _stayConnected = false;
        _client.Dispose();
        _connectTask?.Dispose();
        return ValueTask.CompletedTask;
    }

    public Task Handle(MqttPublishMessageCommand message)
    {
        if (!_client.IsConnected)
        {
            _logger.LogWarning("Mqtt client not connected. Message queued.");
            _queuedMessages.Enqueue(message);
            return Task.CompletedTask;
        }

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
