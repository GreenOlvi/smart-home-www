using CSharpFunctionalExtensions;
using SmartHomeWWW.Core.Domain.Entities;
using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.Telegram.Authorisation
{
    public interface IAuthorisationService
    {
        Task<bool> CanUserRunCommand(long userId, string cmd);
        Task<bool> CanUserDo(long userId, AuthorizedActions action);
        Task<Maybe<TelegramUser>> AddNewUser(Contact contact);
    }
}