using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Utils.Functional;
using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.TelegramBotModule.Authorisation;

public interface IAuthorisationService
{
    bool CanUserRunCommand(long userId, Type command);
    Task<bool> CanUserDo(long userId, AuthorizedActions action);
    Task<Option<TelegramUser>> AddNewUser(Contact contact);
}
