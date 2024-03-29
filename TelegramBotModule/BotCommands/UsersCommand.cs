﻿using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.TelegramBotModule.Messages.Commands;
using System.Text;
using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.TelegramBotModule.BotCommands;

public class UsersCommand(IMessageBus bus, IDbContextFactory<SmartHomeDbContext> dbContextFactory) : ITelegramBotCommand
{
    private readonly IMessageBus _bus = bus;
    private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory = dbContextFactory;

    public Task Run(Message message, CancellationToken cancellationToken = default)
    {
        var args = message.Text?.Split(' ') ?? [];

        if (args.Length < 1)
        {
            return Task.CompletedTask;
        }

        if (args.Length == 1)
        {
            return ListUsers(message);
        }

        return Task.CompletedTask;
    }

    private async Task ListUsers(Message message)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var users = context.TelegramUsers.OrderBy(u => u.Username);

        var sb = new StringBuilder();
        sb.AppendLine("Telegram users in db:");

        var i = 1;
        foreach (var user in users)
        {
            _ = sb.AppendLine($"{i}. {user.Username} [{user.UserType}]");
            i++;
        }

        _bus.Publish(new TelegramSendTextMessageCommand
        {
            ChatId = message.Chat.Id,
            Text = sb.ToString(),
        });
    }
}
