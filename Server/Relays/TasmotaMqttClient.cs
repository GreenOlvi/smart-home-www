using CSharpFunctionalExtensions;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Server.Messages.Events;
using System.Collections.Concurrent;
using System.Text.Json;

namespace SmartHomeWWW.Server.Relays
{
    public class TasmotaMqttClient : ITasmotaClient,
        IMessageHandler<MqttMessageReceivedEvent>
    {
        public TasmotaMqttClient(ILogger<TasmotaMqttClient> logger, IMessageBus bus, string deviceId)
        {
            _logger = logger;
            _bus = bus;
            _bus.Subscribe(this);
            _deviceId = deviceId;

            _bus.Subscribe(this);
            _bus.Publish(new MqttSubscribeToTopicCommand { Topic = $"stat/{deviceId}/+" });
        }

        private readonly ILogger<TasmotaMqttClient> _logger;
        private readonly string _deviceId;
        private readonly IMessageBus _bus;
        private readonly ConcurrentQueue<TaskCompletionSource<Maybe<JsonDocument>>> _waiting = new();

        public Task<Maybe<JsonDocument>> ExecuteCommandAsync(string command, string value)
        {
            var tcs = new TaskCompletionSource<Maybe<JsonDocument>>();
            _waiting.Enqueue(tcs);

            _bus.Publish(new MqttPublishMessageCommand
            {
                Topic = $"cmnd/{_deviceId}/{command}",
                Payload = value,
            });

            return tcs.Task;
        }

        public Task<Maybe<JsonDocument>> GetValueAsync(string command)
        {
            var tcs = new TaskCompletionSource<Maybe<JsonDocument>>();
            _waiting.Enqueue(tcs);

            _bus.Publish(new MqttPublishMessageCommand
            {
                Topic = $"cmnd/{_deviceId}/{command}",
            });

            return tcs.Task;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public Task Handle(MqttMessageReceivedEvent message)
        {
            if (!message.Topic.StartsWith($"stat/{_deviceId}/RESULT", StringComparison.InvariantCultureIgnoreCase))
            {
                return Task.CompletedTask;
            }

            _logger.LogDebug("Waiting for result: {count}", _waiting.Count);
            if (_waiting.TryDequeue(out var tcs))
            {
                var doc = JsonDocument.Parse(message.Payload);
                tcs.SetResult(doc);
            }

            return Task.CompletedTask;
        }
    }
}
