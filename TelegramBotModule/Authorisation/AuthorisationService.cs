using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.Utils.Functional;
using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.TelegramBotModule.Authorisation;

public class AuthorisationService : IAuthorisationService
{
    private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;

    public AuthorisationService(IDbContextFactory<SmartHomeDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public bool CanUserRunCommand(long userId, Type command)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var user = context.TelegramUsers.FirstOrDefault(u => u.TelegramId == userId);
        if (user is null)
        {
            return false;
        }

        if (!command.IsAssignableTo(typeof(ITelegramBotCommand)))
        {
            return false;
        }

        if (!TryGetCommandAction(command, out var action))
        {
            return false;
        }

        return CanUserDoAction(user, action);
    }

    public async Task<bool> CanUserDo(long userId, AuthorizedActions action)
    {
        var u = await FetchUser(userId);
        return u.Match(
            x => CanUserDoAction(x.Value, action),
            x => false);
    }

    public async Task<Option<TelegramUser>> AddNewUser(Contact contact)
    {
        if (contact.UserId is null)
        {
            return new Option<TelegramUser>.None();
        }

        var user = new TelegramUser
        {
            TelegramId = contact.UserId.Value,
            Username = contact.FirstName,
            UserType = "User",
        };

        using var context = await _dbContextFactory.CreateDbContextAsync();
        await context.TelegramUsers.AddAsync(user);
        await context.SaveChangesAsync();
        return new Option<TelegramUser>.Some(user);
    }

    private static bool TryGetCommandAction(Type command, out AuthorizedActions action) =>
        Enum.TryParse($"Run_{command.Name}", out action);

    private static bool CanUserDoAction(TelegramUser user, AuthorizedActions action) => action switch
    {
        AuthorizedActions.RunPingCommand => true,
        AuthorizedActions.RunDelayedPingCommand => true,
        AuthorizedActions.RunUsersCommand => user.UserType == "Owner",
        AuthorizedActions.AddNewUser => user.UserType == "Owner",
        _ => false,
    };

    private async Task<Option<TelegramUser>> FetchUser(long userId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var user = await context.TelegramUsers.FirstOrDefaultAsync(u => u.TelegramId == userId);
        return user == null
            ? new Option<TelegramUser>.None()
            : new Option<TelegramUser>.Some(user);
    }
}
