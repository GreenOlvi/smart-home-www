using MQTTnet.Client;
using MQTTnet.Client.Options;
using SmartHomeWWW.Server.Messages;
using System.Text;

namespace SmartHomeWWW.Server.Mqtt
{
    public class MqttClientHostedService : IHostedService, IAsyncDisposable
    {
        public MqttClientHostedService(ILogger<MqttClientHostedService> logger, IMqttClientFactory clientFactory, IMqttClientOptions options)
        {
            _logger = logger;
            _client = clientFactory.CreateMqttClient();
            _options = options;

            _client.UseConnectedHandler(e =>
            {
                _logger.LogInformation("Mqtt client connected");
                _client.SubscribeAsync("#");
            });

            _client.UseDisconnectedHandler(e => _logger.LogInformation("Mqtt client disconnected"));
            _client.UseApplicationMessageReceivedHandler(e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                _logger.LogInformation("Mqtt message received:\n{topic}\n{payload}", topic, payload);
            });
        }

        private readonly ILogger<MqttClientHostedService> _logger;
        private readonly IMqttClient _client;
        private readonly IMqttClientOptions _options;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Mqtt service started");
            await _client.ConnectAsync(_options, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Mqtt service stopped");
            await _client.DisconnectAsync(cancellationToken: cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            _client.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
