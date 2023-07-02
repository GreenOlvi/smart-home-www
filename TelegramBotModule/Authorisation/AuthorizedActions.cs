namespace SmartHomeWWW.Server.TelegramBotModule.Authorisation;

public enum AuthorizedActions
{
    Invalid = 0,
    RunPingCommand,
    RunDelayedPingCommand,
    RunUsersCommand,
    AddNewUser,
}
