using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using SmartHomeWWW.Core.Domain.Repositories;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.Hubs;
using SmartHomeWWW.Server.Infrastructure;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Server.Messages.Events;
using SmartHomeWWW.Server.TelegramBotModule;
using SmartHomeWWW.Server.TelegramBotModule.Messages.Commands;
using System.Text.Json;
using Telegram.Bot.Types.Enums;

namespace SmartHomeWWW.Server.Weather;

public sealed class WeatherAdapterJob : IOrchestratorJob, IMessageHandler<WeatherUpdatedEvent>, IMessageHandler<MqttMessageReceivedEvent>
{
    private const string WeatherTopic = "env/out/weather";

    private readonly ILogger<WeatherAdapterJob> _logger;
    private readonly IMessageBus _bus;
    private readonly IHubConnection _hubConnection;
    private readonly TelegramConfig _telegramConfig;
    private readonly IKeyValueStore _cache;
    private readonly IWeatherReportRepository _weatherReportRepository;
    private readonly SmartHomeDbContext _db;

    public WeatherAdapterJob(ILogger<WeatherAdapterJob> logger, IMessageBus bus, IHubConnection hubConnection, TelegramConfig telegramConfig,
        IKeyValueStore cache, IWeatherReportRepository weatherReportRepository, SmartHomeDbContext db)
    {
        _logger = logger;
        _bus = bus;
        _hubConnection = hubConnection;
        _telegramConfig = telegramConfig;
        _cache = cache;
        _weatherReportRepository = weatherReportRepository;
        _db = db;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public async Task Handle(WeatherUpdatedEvent message)
    {
        await _hubConnection.SendAsync("UpdateWeather", message.Weather);

        if (message.Weather.Alerts.Any())
        {
            await NotifyAlerts(message.Weather.Alerts);
        }
    }

    private async Task NotifyAlerts(IEnumerable<WeatherAlert> alerts)
    {
        foreach (var alert in alerts)
        {
            var key = $"WeatherAlertNotified_{alert.GetHashCode()}";
            if (!await _cache.ContainsKeyAsync(key))
            {
                _logger.LogInformation("New weather alert. Sending notification.");
                _bus.Publish(FormatAlert(alert));
                await _cache.AddValueAsync(key, alert, TimeSpan.FromDays(7));
            }
            else
            {
                _logger.LogDebug("Weather alert notification already sent. Skipping.");
            }
        }
    }

    private TelegramSendTextMessageCommand FormatAlert(WeatherAlert alert) => new()
    {
        ChatId = _telegramConfig.OwnerId,
        Text = $"{alert.Event}\r\n{alert.SenderName}\r\n{alert.Start.ToLocalTime()} - {alert.End.ToLocalTime()}\r\n{alert.Description}",
        ParseMode = ParseMode.Html,
    };

    public Task Start(CancellationToken cancellationToken)
    {
        _bus.Subscribe<WeatherUpdatedEvent>(this);
        _bus.Subscribe<MqttMessageReceivedEvent>(this);
        _bus.Publish(new MqttSubscribeToTopicCommand { Topic = WeatherTopic });
        return Task.CompletedTask;
    }

    public Task Stop(CancellationToken cancellationToken)
    {
        _bus.Unsubscribe<WeatherUpdatedEvent>(this);
        _bus.Unsubscribe<MqttMessageReceivedEvent>(this);
        return Task.CompletedTask;
    }

    public async Task Handle(MqttMessageReceivedEvent message)
    {
        if (message.Topic != WeatherTopic)
        {
            return;
        }

        var weather = JsonSerializer.Deserialize<WeatherReport?>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (weather is null)
        {
            _logger.LogWarning("Could not parse weather data");
            _logger.LogDebug("Payload: {Payload}", message.Payload);
            return;
        }

        _logger.LogInformation("Received new weather data");

        await _weatherReportRepository.SaveWeatherReport(weather.Value);
        await _db.SaveChangesAsync();

        _bus.Publish(new WeatherUpdatedEvent { Type = "current", Weather = weather.Value });
    }
}
