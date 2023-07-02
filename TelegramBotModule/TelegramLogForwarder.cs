using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Core.MessageBus.Events;
using SmartHomeWWW.Server.TelegramBotModule.Messages.Commands;
using Telegram.Bot.Types.Enums;

namespace SmartHomeWWW.Server.TelegramBotModule;

public sealed class TelegramLogForwarder : IHostedService, IMessageHandler<LogEvent>
{
    private const string _defaultCategory = "default";

    private readonly IMessageBus _messageBus;
    private readonly long _ownerId;

    private readonly Dictionary<string, LogLevel> Levels = new()
    {
        { _defaultCategory, LogLevel.Warning },
    };

    public TelegramLogForwarder(IMessageBus messageBus, long ownerId)
    {
        _messageBus = messageBus;
        _ownerId = ownerId;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _messageBus.Subscribe(this);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _messageBus.Unsubscribe(this);
        return Task.CompletedTask;
    }

    private bool IsLevelValid(string category, LogLevel level) =>
        Levels.TryGetValue(category, out var minLevel)
            ? minLevel <= level
            : Levels[_defaultCategory] <= level;

    public Task Handle(LogEvent message)
    {
        if (IsLevelValid(message.Category, message.Level))
        {
            _messageBus.Publish(CreateMessage(message));
        }

        return Task.CompletedTask;
    }

    private TelegramSendTextMessageCommand CreateMessage(LogEvent log) => new()
    {
        ChatId = _ownerId,
        Text = $"{LogIcons[log.Level]} [{log.Category}]\r\n{log.Message}",
        ParseMode = ParseMode.Html,
    };

    private static readonly Dictionary<LogLevel, string> LogIcons = new()
    {
        { LogLevel.Trace, "🔍" },
        { LogLevel.Debug, "🪲" },
        { LogLevel.Information, "ℹ️" },
        { LogLevel.Warning, "⚠️" },
        { LogLevel.Error, "🛑" },
    };
}
