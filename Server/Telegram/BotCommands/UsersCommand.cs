using System.Text;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Commands;
using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.Telegram.BotCommands;

public class UsersCommand : ITelegramBotCommand
{
    private readonly IMessageBus _bus;
    private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;

    public UsersCommand(IMessageBus bus, IDbContextFactory<SmartHomeDbContext> dbContextFactory)
    {
        _bus = bus;
        _dbContextFactory = dbContextFactory;
    }

    public Task Run(Message message, CancellationToken cancellationToken = default)
    {
        var args = message.Text?.Split(' ') ?? Array.Empty<string>();

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
