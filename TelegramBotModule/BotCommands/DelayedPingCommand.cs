﻿using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.TelegramBotModule.Messages.Commands;
using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.TelegramBotModule.BotCommands;

public class DelayedPingCommand(IMessageBus bus) : ITelegramBotCommand
{
    private readonly IMessageBus _bus = bus;

    public async Task Run(Message message, CancellationToken cancellationToken)
    {
        var msg = message.Text?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? [];

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
