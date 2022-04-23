using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Server.Messages.Events;
using SmartHomeWWW.Server.Telegram.Authorisation;
using SmartHomeWWW.Server.Telegram.BotCommands;
using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.Telegram
{
    public sealed class TelegramBotJob : IOrchestratorJob,
        IMessageHandler<TelegramMessageReceivedEvent>
    {
        public TelegramBotJob(ILogger<TelegramBotJob> logger, IMessageBus bus, IAuthorisationService authorisationService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _bus = bus;
            _authService = authorisationService;
            _commandRegistry = new(serviceProvider);
            RegisterCommands();
        }

        private readonly ILogger<TelegramBotJob> _logger;
        private readonly IMessageBus _bus;
        private readonly CommandRegistry _commandRegistry;
        private readonly IAuthorisationService _authService;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private void RegisterCommands()
        {
            _commandRegistry.AddCommand<PingCommand>("ping");
            _commandRegistry.AddCommand<DelayedPingCommand>("pingd");
            _commandRegistry.AddCommand<UsersCommand>("users");
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
            _logger.LogWarning("Stopping TelegramBotJob");
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        public async Task Handle(TelegramMessageReceivedEvent message)
        {
            var text = message.Message.Text ?? string.Empty;
            var cmd = text.Split(' ')[0];

            if (string.IsNullOrEmpty(cmd))
            {
                await HandleMessageWithoutText(message);
                return;
            }

            if (await _authService.CanUserRunCommand(message.SenderId, cmd))
            {
                if (_commandRegistry.TryGetCommand(cmd, out var command))
                {
                    await command.Run(message.Message,  _cancellationTokenSource.Token);
                }
                else
                {
                    await HandleUnknownCommand(cmd, message.Message);
                }
            }
            else
            {
                await HandleUnauthorizedCommand(cmd, message.Message);
            }
        }

        private async Task HandleMessageWithoutText(TelegramMessageReceivedEvent message)
        {
            if (message.Message.Contact is not null)
            {
                if (await _authService.CanUserDo(message.SenderId, AuthorizedActions.AddNewUser))
                {
                    var contact = message.Message.Contact;
                    var user = await _authService.AddNewUser(contact);
                    if (user.HasValue)
                    {
                        _bus.Publish(new TelegramRefreshAllowedUsersCommand());
                        _bus.Publish(new TelegramSendTextMessageCommand
                        {
                            ChatId = message.SenderId,
                            ReplyToMessageId = message.Message.MessageId,
                            Text = $"User '{user.Value.Username}' added",
                        });
                    }
                    else
                    {
                        _bus.Publish(new TelegramSendTextMessageCommand
                        {
                            ChatId = message.SenderId,
                            ReplyToMessageId = message.Message.MessageId,
                            Text = $"User not added",
                        });
                    }
                }
            }
        }

        private Task HandleUnknownCommand(string cmd, Message message)
        {
            _bus.Publish(new TelegramSendTextMessageCommand
            {
                ChatId = message.Chat.Id,
                ReplyToMessageId = message.MessageId,
                Text = $"Unknown command '{cmd}'",
            });

            _logger.LogWarning("User '{user}' sent unknown command '{cmd}'", message.From?.ToString(), cmd);
            return Task.CompletedTask;
        }

        private Task HandleUnauthorizedCommand(string cmd, Message message)
        {
            _bus.Publish(new TelegramSendTextMessageCommand
            {
                ChatId = message.Chat.Id,
                ReplyToMessageId = message.MessageId,
                Text = $"Unauthorized command '{cmd}'",
            });

            _logger.LogError("User '{user}' tried running unauthorized command '{cmd}'", message.From?.ToString(), cmd);
            return Task.CompletedTask;
        }
    }
}
