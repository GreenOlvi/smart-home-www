using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Infrastructure;

namespace SmartHomeWWW.Server.Telegram.Authorisation
{
    public class AuthorisationService : IAuthorisationService
    {
        public AuthorisationService(IDbContextFactory<SmartHomeDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;

        public async Task<bool> CanUserRunCommand(long userId, string cmd)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var user = await context.TelegramUsers.FirstOrDefaultAsync(u => u.TelegramId == userId);

            if (user == null)
            {
                return false;
            }

            return true;
        }
    }
}
