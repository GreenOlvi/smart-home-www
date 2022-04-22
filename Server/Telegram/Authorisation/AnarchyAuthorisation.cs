namespace SmartHomeWWW.Server.Telegram.Authorisation
{
    public class AnarchyAuthorisation : IAuthorisationService
    {
        public Task<bool> CanUserRunCommand(long userId, string cmd) => Task.FromResult(true);
    }
}
