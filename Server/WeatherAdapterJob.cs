﻿using Microsoft.AspNetCore.SignalR.Client;
using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using SmartHomeWWW.Server.Config;
using SmartHomeWWW.Server.Infrastructure;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Server.Messages.Events;
using Telegram.Bot.Types.Enums;

namespace SmartHomeWWW.Server;

public sealed class WeatherAdapterJob : IOrchestratorJob, IMessageHandler<WeatherUpdatedEvent>
{
    private readonly ILogger<WeatherAdapterJob> _logger;
    private readonly IMessageBus _bus;
    private readonly HubConnection _hubConnection;
    private readonly TelegramConfig _telegramConfig;
    private readonly IKeyValueStore _cache;

    public WeatherAdapterJob(ILogger<WeatherAdapterJob> logger, IMessageBus bus, HubConnection hubConnection, TelegramConfig telegramConfig, IKeyValueStore cache)
    {
        _logger = logger;
        _bus = bus;
        _hubConnection = hubConnection;
        _telegramConfig = telegramConfig;
        _cache = cache;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public async Task Handle(WeatherUpdatedEvent message)
    {
        if (message.Weather?.Alerts.Any() ?? false)
        {
            await NotifyAlerts(message.Weather.Alerts);
        }

        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            await _hubConnection.StartAsync();
        }

        await _hubConnection.SendAsync("UpdateWeather", message.Weather);
    }

    private async Task NotifyAlerts(WeatherAlert[] alerts)
    {
        foreach (var alert in alerts)
        {
            var key = $"WeatherAlertNotified_{alert.GetHashCode()}";
            if (await _cache.ContainsKeyAsync(key))
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
        _bus.Subscribe(this);
        return Task.CompletedTask;
    }

    public Task Stop(CancellationToken cancellationToken)
    {
        _bus.Unsubscribe(this);
        return Task.CompletedTask;
    }
}