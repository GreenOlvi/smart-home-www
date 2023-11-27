using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.Hubs;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Server.Messages.Events;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SmartHomeWWW.Server.Sensors;

public sealed partial class SensorMonitorJob(ILogger<SensorMonitorJob> logger, IMessageBus bus, IDbContextFactory<SmartHomeDbContext> dbContextFactory,
    IHubConnection hubConnection) : IOrchestratorJob, IMessageHandler<MqttMessageReceivedEvent>
{
    private readonly ILogger<SensorMonitorJob> _logger = logger;
    private readonly IMessageBus _bus = bus;
    private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory = dbContextFactory;
    private readonly IHubConnection _hubConnection = hubConnection;

    public Task Start(CancellationToken cancellationToken = default)
    {
        _bus.Subscribe(this);
        _bus.Publish(new MqttSubscribeToTopicCommand { Topic = "env/+/data" });
        _logger.LogDebug("Sensor monitor job started");
        return Task.CompletedTask;
    }

    public Task Stop(CancellationToken cancellationToken = default)
    {
        _bus.Unsubscribe(this);
        _logger.LogDebug("Sensor monitor job stopped");
        return Task.CompletedTask;
    }

    private static readonly Regex TopicMatch = BuildTopicMatch();

    public Task Handle(MqttMessageReceivedEvent message)
    {
        if (!TopicMatch.IsMatch(message.Topic))
        {
            return Task.CompletedTask;
        }

        _logger.LogDebug("Received data from sensor: {Payload}", message.Payload);
        var data = JsonSerializer.Deserialize<SensorEnvData>(message.Payload);
        if (data.Timestamp == DateTime.MinValue)
        {
            _logger.LogWarning("Could not parse sensor data");
            return Task.CompletedTask;
        }

        _logger.LogInformation("Sensor data: {Data}", data);

        return UpdateSensorAndNotify(data);
    }

    private async Task UpdateSensorAndNotify(SensorEnvData data)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();
        var sensor = await db.Sensors.FirstOrDefaultAsync(s => s.Mac == data.Mac);

        if (sensor is null)
        {
            sensor = new Sensor
            {
                Id = Guid.NewGuid(),
                Mac = data.Mac,
                ChipType = "ESP8266",
            };
            db.Sensors.Add(sensor);
        }

        sensor.FirmwareVersion = data.Version;
        sensor.LastContact = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await _hubConnection.SendUpdateSensor(sensor);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [GeneratedRegex(@"^env/[^//]+/data", RegexOptions.Compiled)]
    private static partial Regex BuildTopicMatch();
}
