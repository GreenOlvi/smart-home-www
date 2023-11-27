using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.TelegramBotModule.Messages.Commands;
using SmartHomeWWW.Server.TelegramBotModule.Messages.Events;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SmartHomeWWW.Server.TelegramBotModule;

public sealed class TelegramBotHostedService : IHostedService, IAsyncDisposable,
    IMessageHandler<TelegramSendTextMessageCommand>,
    IMessageHandler<TelegramRefreshAllowedUsersCommand>
{
    public TelegramBotHostedService(ILogger<TelegramBotHostedService> logger, IHttpClientFactory httpClientFactory, IOptions<TelegramConfig> config,
        IMessageBus messageBus, IDbContextFactory<SmartHomeDbContext> dbContextFactory)
    {
        _logger = logger;
        _config = config.Value;
        _messageBus = messageBus;
        _httpClientFactory = httpClientFactory;
        _dbContextFactory = dbContextFactory;

        var httpClient = _config.HttpClientName is null
            ? _httpClientFactory.CreateClient()
            : _httpClientFactory.CreateClient(_config.HttpClientName);

        _bot = new TelegramBotClient(_config.ApiKey, httpClient);
    }

    private readonly ILogger<TelegramBotHostedService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TelegramConfig _config;
    private readonly IMessageBus _messageBus;
    private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;

    private readonly ITelegramBotClient _bot;
    private readonly HashSet<long> _allowedUsers = [];

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var me = await _bot.GetMeAsync(cancellationToken);
        _logger.LogInformation("Starting {Botname}...", me.Username);

        _bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, cancellationToken: _cancellationTokenSource.Token);

        _messageBus.Subscribe<TelegramSendTextMessageCommand>(this);
        _messageBus.Subscribe<TelegramRefreshAllowedUsersCommand>(this);

        try
        {
            await LoadAllowedUsers();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading allowed users");
        }
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
        _logger.LogWarning(exception, "Caught an exception from TelegramBot: {Message}", exception.Message);
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
