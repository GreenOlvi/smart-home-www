namespace SmartHomeWWW.Server.Telegram.Authorisation
{
    public interface IAuthorisationService
    {
        Task<bool> CanUserRunCommand(long userId, string cmd);
    }
}