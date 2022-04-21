namespace SmartHomeWWW.Server.Telegram.Authentication
{
    public interface IAuthenticationService
    {
        Task<bool> CanUserRunCommand(long userId, string cmd);
    }
}