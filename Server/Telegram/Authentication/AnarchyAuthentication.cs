namespace SmartHomeWWW.Server.Telegram.Authentication
{
    public class AnarchyAuthentication : IAuthenticationService
    {
        public Task<bool> CanUserRunCommand(long userId, string cmd) => Task.FromResult(true);
    }
}
