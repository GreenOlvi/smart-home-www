using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.TelegramBotModule.Messages.Commands;
using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.TelegramBotModule.BotCommands;

public class PingCommand : ITelegramBotCommand
{
    public PingCommand(IMessageBus bus)
    {
        _bus = bus;
    }

    private readonly IMessageBus _bus;

    public Task Run(Message message, CancellationToken cancellationToken)
    {
        _bus.Publish(new TelegramSendTextMessageCommand
        {
            ChatId = message.Chat.Id,
            ReplyToMessageId = message.MessageId,
            Text = "pong",
        });

        return Task.CompletedTask;
    }
}
