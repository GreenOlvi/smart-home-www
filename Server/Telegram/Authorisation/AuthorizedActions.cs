namespace SmartHomeWWW.Server.Telegram.Authorisation;

public enum AuthorizedActions
{
    Invalid = 0,
    RunPingCommand,
    RunDelayedPingCommand,
    RunUsersCommand,
    AddNewUser,
}
