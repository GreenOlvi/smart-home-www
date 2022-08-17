using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Server.Config;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Server.Messages.Events;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SmartHomeWWW.Server.Telegram;

public sealed class TelegramBotHostedService : IHostedService, IAsyncDisposable,
    IMessageHandler<TelegramSendTextMessageCommand>,
    IMessageHandler<TelegramRefreshAllowedUsersCommand>
{
    public TelegramBotHostedService(ILogger<TelegramBotHostedService> logger, HttpClient httpClient, TelegramConfig config,
        IMessageBus messageBus, IDbContextFactory<SmartHomeDbContext> dbContextFactory)
    {
        _logger = logger;
        _config = config;
        _messageBus = messageBus;
        _bot = new TelegramBotClient(_config.ApiKey, httpClient);
        _dbContextFactory = dbContextFactory;
    }

    private readonly ILogger<TelegramBotHostedService> _logger;
    private readonly TelegramConfig _config;
    private readonly TelegramBotClient _bot;
    private readonly IMessageBus _messageBus;
    private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;
    private readonly HashSet<long> _allowedUsers = new ();

    private readonly CancellationTokenSource _cancellationTokenSource = new ();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var me = await _bot.GetMeAsync(cancellationToken);
        _logger.LogInformation("Starting {Botname}...", me.Username);

        _bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, cancellationToken: _cancellationTokenSource.Token);

        _messageBus.Subscribe<TelegramSendTextMessageCommand>(this);
        _messageBus.Subscribe<TelegramRefreshAllowedUsersCommand>(this);

        await LoadAllowedUsers();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var me = await _bot.GetMeAsync(cancellationToken);
        _logger.LogInformation("Stopping {Botname}...", me.Username);

        _messageBus.Unsubscribe<TelegramSendTextMessageCommand>(this);

        _cancellationTokenSource.Cancel();
    }

    public ValueTask DisposeAsync()
    {
        _cancellationTokenSource.Dispose();
        return ValueTask.CompletedTask;
    }

    private async Task LoadAllowedUsers()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        _allowedUsers.Clear();
        var dbUsers = context.TelegramUsers
            .Where(u => !string.IsNullOrEmpty(u.UserType))
            .Select(u => u.TelegramId);

        foreach (var u in dbUsers)
        {
            _allowedUsers.Add(u);
        }
    }

    private Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message || update.Message is null)
        {
            _logger.LogInformation("Update {Type} ignored", update.Type);
            return Task.CompletedTask;
        }

        var message = update.Message;
        if (message.From is null || !_allowedUsers.Contains(message.From.Id))
        {
            _logger.LogError("Received text message from unknown user '{User}': '{Message}'",
                message.From?.ToString() ?? "unknown",
                message.Text ?? string.Empty);

            return Task.CompletedTask;
        }

        _logger.LogDebug("Received text message from '{User}': '{Message}'", message.From.ToString(), message.Text ?? string.Empty);

        _messageBus.Publish(new TelegramMessageReceivedEvent
        {
            ChatId = message.Chat.Id,
            SenderId = message.From.Id,
            Message = message,
        });

        return Task.CompletedTask;
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Caught an exception from TelegramBot");
        return Task.CompletedTask;
    }

    public Task Handle(TelegramSendTextMessageCommand message)
    {
        _logger.LogDebug("Sending message to {Id} with text '{Text}'", message.ChatId, message.Text);
        return _bot.SendTextMessageAsync(
            message.ChatId,
            message.Text,
            parseMode: message.ParseMode,
            disableWebPagePreview: message.DisableWebPagePreview,
            disableNotification: message.DisableNotification,
            replyToMessageId: message.ReplyToMessageId,
            allowSendingWithoutReply: message.AllowSendingWithoutReply,
            cancellationToken: _cancellationTokenSource.Token);
    }

    public Task Handle(TelegramRefreshAllowedUsersCommand message) => LoadAllowedUsers();
}
