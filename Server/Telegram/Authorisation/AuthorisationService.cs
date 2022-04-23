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

        public async Task<bool> CanUserRunCommand(long userId, string cmd) => (await FetchUser(userId)).HasValue;

        public async Task<bool> CanUserDo(long userId, AuthorizedActions action)
        {
            var u = await FetchUser(userId);
            if (u.HasNoValue)
            {
                return false;
            }

            var user = u.Value;
            return action switch
            {
                AuthorizedActions.AddNewUser => user.UserType == "Owner",
                AuthorizedActions.ListUsers => user.UserType == "Owner",
                _ => false,
            };
        }

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
