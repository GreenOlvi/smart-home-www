using SmartHomeWWW.Server.Config;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Commands;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.Telegram
{
    public class TelegramBotHostedService : IHostedService, IAsyncDisposable,
        IMessageHandler<TelegramSendTextMessageCommand>
    {
        public TelegramBotHostedService(ILogger<TelegramBotHostedService> logger, HttpClient httpClient, TelegramConfig config, IMessageBus messageBus)
        {
            _logger = logger;
            _config = config;
            _messageBus = messageBus;
            _bot = new TelegramBotClient(_config.ApiKey, httpClient);
        }

        private readonly ILogger<TelegramBotHostedService> _logger;
        private readonly TelegramConfig _config;
        private readonly TelegramBotClient _bot;
        private readonly IMessageBus _messageBus;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly ReceiverOptions _receiverOptions = new()
        {
            AllowedUpdates = { },
        };

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var me = await _bot.GetMeAsync(cancellationToken);
            _logger.LogInformation("Starting {botname}...", me.Username);

            _bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, _receiverOptions, cancellationToken);
            //await _bot.SendTextMessageAsync(_config.OwnerId, "I'm online", cancellationToken: cancellationToken);

            _messageBus.Subscribe<TelegramSendTextMessageCommand>(this);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var me = await _bot.GetMeAsync(cancellationToken);
            _logger.LogInformation("Stopping {botname}...", me.Username);

            _messageBus.Unsubscribe<TelegramSendTextMessageCommand>(this);

            _cancellationTokenSource.Cancel();
            //await _bot.SendTextMessageAsync(_config.OwnerId, "Shutting down", cancellationToken: cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            _cancellationTokenSource.Dispose();
            return ValueTask.CompletedTask;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is Message message)
            {
                _logger.LogInformation("Received text message from '{user}': '{message}'", message.From?.Username ?? "unknown", message.Text ?? string.Empty);

                if (message.Text == "ping")
                {
                    await bot.SendTextMessageAsync(message.Chat, "pong", cancellationToken: cancellationToken);
                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat, "Hello", cancellationToken: cancellationToken);
                }
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Caught an exception");
            return Task.CompletedTask;
        }

        public async Task Handle(TelegramSendTextMessageCommand message)
        {
            await _bot.SendTextMessageAsync(message.ChatId, message.Text, cancellationToken: _cancellationTokenSource.Token);
        }
    }
}
