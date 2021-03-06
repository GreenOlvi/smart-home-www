using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Infrastructure;
using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.Telegram.Authorisation
{
    public class AuthorisationService : IAuthorisationService
    {
        public AuthorisationService(IDbContextFactory<SmartHomeDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;

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

        private static bool TryGetCommandAction(Type command, out AuthorizedActions action) =>
            Enum.TryParse($"Run_{command.Name}", out action);

        public async Task<bool> CanUserDo(long userId, AuthorizedActions action)
        {
            var u = await FetchUser(userId);
            return u.HasValue && CanUserDoAction(u.Value, action);
        }

        private static bool CanUserDoAction(TelegramUser user, AuthorizedActions action) => action switch
        {
            AuthorizedActions.Run_PingCommand => true,
            AuthorizedActions.Run_DelayedPingCommand => true,
            AuthorizedActions.Run_UsersCommand => user.UserType == "Owner",
            AuthorizedActions.AddNewUser => user.UserType == "Owner",
            _ => false,
        };

        private async Task<Maybe<TelegramUser>> FetchUser(long userId)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var user = await context.TelegramUsers.FirstOrDefaultAsync(u => u.TelegramId == userId);

            if (user == null)
            {
                return Maybe<TelegramUser>.None;
            }

            return user;
        }

        public async Task<Maybe<TelegramUser>> AddNewUser(Contact contact)
        {
            if (contact.UserId is null)
            {
                return Maybe<TelegramUser>.None;
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
            return user;
        }
    }
}
