using System.Net;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Server.Messages.Events;
using Telegram.Bot.Types.Enums;

namespace SmartHomeWWW.Server.TelegramBot;

public sealed class TelegramLogForwarder : IOrchestratorJob, IMessageHandler<LogEvent>
{
    private const string _defaultCategory = "default";

    private readonly IMessageBus _messageBus;
    private readonly long _ownerId;

    private readonly Dictionary<string, LogLevel> Levels = new()
    {
        {_defaultCategory, LogLevel.Warning },
    };

    public TelegramLogForwarder(IMessageBus messageBus, long ownerId)
    {
        _messageBus = messageBus;
        _ownerId = ownerId;
    }

    public Task Start(CancellationToken cancellationToken)
    {
        _messageBus.Subscribe(this);
        return Task.CompletedTask;
    }

    public Task Stop(CancellationToken cancellationToken)
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

    private static Dictionary<LogLevel, string> LogIcons = new()
    {
        { LogLevel.Trace, "🔍" },
        { LogLevel.Debug, "🪲" },
        { LogLevel.Information, "ℹ️" },
        { LogLevel.Warning, "⚠️" },
        { LogLevel.Error, "🛑" },
    };

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
