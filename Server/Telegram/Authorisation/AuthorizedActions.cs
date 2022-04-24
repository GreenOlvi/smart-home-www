namespace SmartHomeWWW.Server.Telegram.Authorisation
{
    public enum AuthorizedActions
    {
        Invalid = 0,
        Run_PingCommand,
        Run_DelayedPingCommand,
        Run_UsersCommand,
        AddNewUser,
    }
}