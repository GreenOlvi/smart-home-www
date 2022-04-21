using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Server.Messages.Events;
using SmartHomeWWW.Server.Telegram.BotCommands;
using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.Telegram
{
    public sealed class TelegramBotJob : IOrchestratorJob,
        IMessageHandler<TelegramMessageReceivedEvent>
    {
        public TelegramBotJob(ILogger<TelegramBotJob> logger, IMessageBus bus, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _bus = bus;
            _commandRegistry = new(serviceProvider);
            RegisterCommands();
        }

        private readonly ILogger<TelegramBotJob> _logger;
        private readonly IMessageBus _bus;
        private readonly CommandRegistry _commandRegistry;

        private void RegisterCommands()
        {
            _commandRegistry.AddCommand<PingCommand>("ping");
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public Task Start(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Starting TelegramBotJob");
            _bus.Subscribe(this);
            return Task.CompletedTask;
        }

        public Task Stop(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Stopping TelegramBotJob");
            return Task.CompletedTask;
        }

        public Task Handle(TelegramMessageReceivedEvent message)
        {
            var text = message.Message.Text ?? string.Empty;
            var cmd = text.Split(' ')[0];

            return _commandRegistry.TryGetCommand(cmd, out var command)
                ? command.Run(message.Message)
                : HandleUnknownCommand(cmd, message.Message);
        }

        private Task HandleUnknownCommand(string cmd, Message message)
        {
            _bus.Publish(new TelegramSendTextMessageCommand
            {
                ChatId = message.Chat.Id,
                ReplyToMessageId = message.MessageId,
                Text = $"Unknown command '{cmd}'",
            });
            _logger.LogInformation("User {user} sent unknown command '{cmd}'", message.From?.ToString(), cmd);
            return Task.CompletedTask;
        }
    }
}
