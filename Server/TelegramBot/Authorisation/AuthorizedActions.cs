namespace SmartHomeWWW.Server.TelegramBot.Authorisation;

public enum AuthorizedActions
{
    Invalid = 0,
    RunPingCommand,
    RunDelayedPingCommand,
    RunUsersCommand,
    AddNewUser,
    RunUrlStore,
}
