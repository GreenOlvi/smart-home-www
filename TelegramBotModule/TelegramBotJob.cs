using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.TelegramBotModule.Authorisation;
using SmartHomeWWW.Server.TelegramBotModule.BotCommands;
using SmartHomeWWW.Server.TelegramBotModule.Messages.Commands;
using SmartHomeWWW.Server.TelegramBotModule.Messages.Events;
using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.TelegramBotModule;

public sealed class TelegramBotCommandHandlerJob : IHostedService,
    IMessageHandler<TelegramMessageReceivedEvent>,
    IConsumer<TelegramMessageReceivedEvent>,
    IDisposable
{
    public TelegramBotCommandHandlerJob(ILogger<TelegramBotCommandHandlerJob> logger, IMessageBus bus, IAuthorisationService authorisationService, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _bus = bus;
        _authService = authorisationService;
        _commandRegistry = new(serviceProvider);
        RegisterCommands();
    }

    private readonly ILogger<TelegramBotCommandHandlerJob> _logger;
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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting TelegramBotJob");
        _bus.Subscribe(this);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Stopping TelegramBotJob");
        _bus.Unsubscribe(this);
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    public Task Handle(TelegramMessageReceivedEvent message)
    {
        var text = message.Message.Text ?? string.Empty;
        var cmd = text.Split(' ')[0];

        if (string.IsNullOrEmpty(cmd))
        {
            return HandleMessageWithoutText(message);
        }

        if (!_commandRegistry.TryGetCommand(cmd, out var command))
        {
            return HandleUnknownCommand(cmd, message.Message);
        }

        if (!_authService.CanUserRunCommand(message.SenderId, command))
        {
            return HandleUnauthorizedCommand(cmd, message.Message);
        }

        if (!_commandRegistry.TryCreateCommandInstance(command, out var instance))
        {
            return HandleCouldNotCreateCommandInstance(cmd, message.Message);
        }

        return instance.Run(message.Message, _cancellationTokenSource.Token);
    }

    public Task Consume(ConsumeContext<TelegramMessageReceivedEvent> context) => Handle(context.Message);

    private async Task HandleMessageWithoutText(TelegramMessageReceivedEvent message)
    {
        if (message.Message.Contact is null || !await _authService.CanUserDo(message.SenderId, AuthorizedActions.AddNewUser))
        {
            return;
        }

        var contact = message.Message.Contact;
        var user = await _authService.AddNewUser(contact);
        user.Match(
            u =>
            {
                _bus.Publish(new TelegramRefreshAllowedUsersCommand());
                _bus.Publish(new TelegramSendTextMessageCommand
                {
                    ChatId = message.SenderId,
                    ReplyToMessageId = message.Message.MessageId,
                    Text = $"User '{u.Value.Username}' added",
                });
            },
            _ =>
            {
                _bus.Publish(new TelegramSendTextMessageCommand
                {
                    ChatId = message.SenderId,
                    ReplyToMessageId = message.Message.MessageId,
                    Text = $"User not added",
                });
            });
    }

    private Task HandleUnknownCommand(string cmd, Message message)
    {
        _bus.Publish(new TelegramSendTextMessageCommand
        {
            ChatId = message.Chat.Id,
            ReplyToMessageId = message.MessageId,
            Text = $"Unknown command '{cmd}'",
        });

        _logger.LogWarning("User '{User}' sent unknown command '{Cmd}'", message.From?.ToString(), cmd);
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

        _logger.LogError("User '{User}' tried running unauthorized command '{Cmd}'", message.From?.ToString(), cmd);
        return Task.CompletedTask;
    }

    private Task HandleCouldNotCreateCommandInstance(string cmd, Message message)
    {
        _bus.Publish(new TelegramSendTextMessageCommand
        {
            ChatId = message.Chat.Id,
            ReplyToMessageId = message.MessageId,
            Text = $"Could not create instance of '{cmd}'",
        });

        _logger.LogError("Could not create instance of '{Cmd}' for user '{User}'", cmd, message.From?.ToString());
        return Task.CompletedTask;
    }

    public void Dispose() => _cancellationTokenSource.Dispose();
}
