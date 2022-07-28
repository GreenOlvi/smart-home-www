using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Commands;
using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.Telegram.BotCommands;

public class DelayedPingCommand : ITelegramBotCommand
{
    public DelayedPingCommand(IMessageBus bus)
    {
        _bus = bus;
    }

    private readonly IMessageBus _bus;

    public async Task Run(Message message, CancellationToken cancellationToken)
    {
        var msg = message.Text?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? Array.Empty<string>();

        if (!long.TryParse(msg[1], out var seconds))
        {
            _bus.Publish(new TelegramSendTextMessageCommand
            {
                ChatId = message.Chat.Id,
                ReplyToMessageId = message.MessageId,
                Text = $"Could not parse '{msg[1]}' to a number",
            });
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(seconds), cancellationToken);

        _bus.Publish(new TelegramSendTextMessageCommand
        {
            ChatId = message.Chat.Id,
            ReplyToMessageId = message.MessageId,
            Text = "pong",
        });
    }
}
