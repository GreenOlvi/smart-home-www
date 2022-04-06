using SmartHomeWWW.Server.Config;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.Telegram
{
    public class TelegramBotHostedService : IHostedService, IAsyncDisposable
    {
        public TelegramBotHostedService(ILogger<TelegramBotHostedService> logger, HttpClient httpClient, TelegramConfig config)
        {
            _logger = logger;
            _config = config;
            _bot = new TelegramBotClient(_config.ApiKey, httpClient);
        }

        private readonly ILogger<TelegramBotHostedService> _logger;
        private readonly TelegramConfig _config;
        private readonly TelegramBotClient _bot;
        private readonly ReceiverOptions _receiverOptions = new()
        {
            AllowedUpdates = { },
        };

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var me = await _bot.GetMeAsync(cancellationToken);
            _logger.LogInformation("Starting {botname}...", me.Username);

            _bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, _receiverOptions, cancellationToken);

            await _bot.SendTextMessageAsync(_config.OwnerId, "I'm online", cancellationToken: cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var me = await _bot.GetMeAsync(cancellationToken);
            _logger.LogInformation("Stopping {botname}...", me.Username);
            await _bot.SendTextMessageAsync(_config.OwnerId, "Shutting down",
                cancellationToken: cancellationToken);
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

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
            return Task.CompletedTask;
        }
    }
}
